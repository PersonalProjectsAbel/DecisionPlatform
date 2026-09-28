# ADR-0004: Use EventStoreDB as the Event Source and PostgreSQL for Projections

## Status

Accepted — Meeting event-stream persistence, the initial PostgreSQL projections, and the first Python minutes-generation integration are implemented; voting remains pending.

## Context

DecisionPlanner is intended to be a modular monolith with Meetings, Decisions, Voting, and Minutes capabilities. It must retain the history of business changes and support consumers beyond the main .NET API, including a Python FastAPI service that creates AI-assisted minutes drafts.

The initial Meeting implementation persisted current state directly to PostgreSQL through EF Core. That approach did not preserve the complete sequence of business changes as the authoritative record. The project needs a durable event history and a clear separation between the event source of truth, query models, and asynchronous consumers.

## Decision

Use EventStoreDB (currently distributed as KurrentDB) as the authoritative write-side event store. Append business events to streams associated with their aggregate. Commands load and rehydrate the relevant aggregate from its stream, enforce domain rules, and append new events with an expected stream revision so concurrent writes to the same aggregate are detected.

Use PostgreSQL for read-side projections that support API and application queries. .NET projection consumers update these read models from EventStoreDB events. Projections are derived data and must be rebuildable by replaying the event history. They may be eventually consistent with the event store.

The Python FastAPI minutes service consumes the events relevant to minutes generation and produces a draft. The result must re-enter the system through an explicit, versioned contract; it must not write directly to another service's tables. Consumers must tolerate retries and duplicate deliveries.

Do not independently dual-write a business change to EventStoreDB and PostgreSQL. EventStoreDB is the write-side source of truth; PostgreSQL changes as a consequence of event consumption. EventStoreDB is not merely an outbox for this design.

The .NET application remains a modular monolith. Modules communicate through explicit contracts and events where appropriate; this decision does not create a separate deployable service for each module.

## Relationship to ADR-0003

ADR-0003 established EF Core, PostgreSQL, a `DbContext`, and a dedicated migrations project for the initial persistence implementation. This ADR supersedes ADR-0003 only for the write-side source of truth and the role of PostgreSQL: PostgreSQL is a projection store in the target architecture, while EventStoreDB owns the authoritative event streams.

EF Core may remain useful for PostgreSQL projection storage and schema migrations. The earlier EF Core model that saved current Meeting state directly as the authoritative record has been replaced for the Meeting write path by event streams. A hosted .NET consumer projects meeting, participant, topic, proposal, and discussion-entry events into PostgreSQL. Meeting detail GET still reads by replaying KurrentDB; the discussion-entry GET reads from PostgreSQL and is eventually consistent. Any remaining migration of existing authoritative PostgreSQL data into KurrentDB must be planned explicitly; existing data and migration history must not be discarded implicitly.

## Consequences

### Positive

- The system retains the sequence of business facts and can rebuild projections.
- Meeting and future modules can publish events for other modules and independent consumers.
- PostgreSQL read models can be shaped for efficient API queries without becoming a competing write-side source of truth.
- EventStoreDB expected-revision checks expose conflicting concurrent writes to the same aggregate stream.
- The Python service can process relevant events without being coupled to .NET persistence internals.

### Negative

- Event sourcing changes aggregate persistence, command handling, and query design; it is more complex than storing current state only.
- PostgreSQL projections are eventually consistent and require checkpointing, replay, idempotent handlers, and operational monitoring.
- Event schemas are durable contracts and need a versioning and compatibility strategy.
- EventStoreDB and PostgreSQL add separate runtime and operational responsibilities.
- EventStoreDB stream concurrency does not by itself enforce uniqueness across different streams. Business uniqueness requirements need explicit identifiers or another consistency mechanism.
- Existing EF Core current-state persistence requires a deliberate migration path.

## Alternatives Considered

### PostgreSQL as the only source of truth with an outbox

Keep current-state tables authoritative and use a transactional outbox to publish integration events. This is simpler and remains a valid alternative if complete event history and event-sourced aggregates are not required. It was not selected because retaining business history and using EventStoreDB as the event source are part of the target project architecture.

### Write independently to EventStoreDB and PostgreSQL

Rejected because the two writes cannot be assumed to commit atomically. A failure between writes can leave inconsistent copies. PostgreSQL will instead be updated by event-driven projections.

### EventStoreDB for writes and direct event-store queries for all reads

Rejected as the default query strategy because API and reporting needs may require read models shaped differently from aggregate streams. PostgreSQL projections provide a query-oriented store and can be rebuilt.

## Implementation Guidance

The current Meeting projection repository uses parameterized PostgreSQL SQL for its writes, including `INSERT ... ON CONFLICT` upserts. This makes replay and duplicate event delivery safe for the projected rows: applying the same event again converges on the same row instead of failing on a duplicate key. Status changes are also written directly so an event whose prerequisite Meeting row is missing fails visibly and can be retried rather than being silently ignored. The SQL is parameterized through EF Core's interpolated SQL API; event values are not concatenated into SQL text. EF Core remains responsible for connection management and schema migrations. If projection writes later move to tracked EF entities, the replacement must preserve idempotent replay and make missing-row/concurrency behavior explicit.

Each Meeting stream now includes topic, proposal, and discussion-entry events. New meetings receive a General topic. For older streams with no General topic event, rehydration supplies a stable General topic ID derived from the meeting ID and the next successful command persists that topic as an event. Discussion entries require a participant belonging to the meeting and may reference a proposal; a missing proposal means the entry belongs to the topic discussion. The discussion-entry query returns all projected entries by default and accepts optional topic and proposal filters. Its PostgreSQL results are eventually consistent with KurrentDB.

1. Define and version event names, payloads, ownership, and stream identity for each Meeting workflow.
2. Expand the implemented stream-replay and expected-revision pattern across Meeting use cases.
3. Add rebuild and operational procedures for the Meeting projection, including subscription reset and replay.
4. Move Meeting queries to PostgreSQL after confirming all required event types are projected.
5. The Python minutes worker consumes `meeting.completed.v1` through a persistent subscription, rebuilds context from the Meeting stream, and submits a draft to the authenticated .NET API. The API appends the versioned `meeting.minutes-draft-generated.v1` event idempotently by completion event ID. The Meeting GET response reads drafts from the event stream. The worker can run as a separate process or through the opt-in `minutes` Docker Compose profile. See [ADR-0005](0005-python-minutes-worker.md) for service boundaries, deployment, retry behavior, and the flow diagram.
6. Plan migration of any existing PostgreSQL Meeting data needed by the new event stream.
