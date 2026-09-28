# DecisionPlanner

DecisionPlanner is a .NET modular monolith for collaborative decision-making. Its business modules are Meetings, Decisions, Voting, and Minutes. The system is intended to capture decisions and their history, support collaboration and voting, and use AI to help prepare meeting minutes.

## Architecture direction

The target architecture uses events as the durable record of business changes:

```text
.NET modular monolith
  Meetings | Decisions | Voting | Minutes
                  |
                  | append domain events
                  v
             EventStoreDB
               /       \
              /         \
 .NET projections       Python FastAPI service
          |              (AI-assisted minutes)
          v                       |
      PostgreSQL                  | minutes draft/result
                                  v
                           EventStoreDB
```

- **EventStoreDB** is intended to be the source of truth for business events, stored in streams associated with the relevant aggregate.
- **PostgreSQL** is intended for query-friendly projections used by the .NET API. These projections are derived from events, may briefly lag behind the event store, and should be rebuildable by replaying events.
- **.NET projections** consume events and maintain PostgreSQL read models. Commands validate business rules against aggregate state reconstructed from its event stream and append resulting events to EventStoreDB.
- **The Python FastAPI service** consumes relevant events to produce an AI-assisted minutes draft. Its result should be returned as an explicit, versioned event or command, with retries and duplicate delivery handled safely.
- The application should not independently write the same business change to EventStoreDB and PostgreSQL. PostgreSQL is a projection in this design, not a competing write-side source of truth.

See [ADR-0004](docs/adr/0004-eventstoredb-and-postgresql-projections.md) for the decision, trade-offs, and relationship to the current persistence implementation.

## Current implementation status

The repository is an early implementation. Meeting use cases append versioned domain events to KurrentDB (formerly EventStoreDB) and rebuild aggregates by replaying their streams. A hosted .NET projection maintains PostgreSQL meeting, participant, topic, proposal, and discussion-entry tables. Meeting details are still read from KurrentDB; the discussion-entry endpoint reads the PostgreSQL projection and supports optional topic/proposal filters, so it may briefly lag behind writes. Topics and proposals are included in the Meeting stream; a General topic is created for new meetings. Voting is not yet implemented. The Python minutes service now has a provider-neutral AI port with a Gemini adapter, a persistent KurrentDB worker for completed meetings, and an authenticated .NET callback that records versioned minutes-draft events. Meeting GET exposes generated drafts by replaying the stream. The worker can run as a separate host process or through the opt-in `minutes` Docker Compose profile.

The solution currently contains these projects:

- `DecisionPlanner.Domain` — domain model and business rules.
- `DecisionPlanner.Application` — use cases and application abstractions.
- `DecisionPlanner.Infrastructure` — KurrentDB event-stream persistence for Meetings and EF Core/PostgreSQL infrastructure reserved for read projections.
- `DecisionPlanner.Migrations` — current EF Core migration history.
- `DecisionPlanner.Api` — HTTP API and application composition.

## Architectural decisions

Architecture Decision Records are in [`docs/adr`](docs/adr/). Start with:

- [ADR-0001: Use Clean Architecture](docs/adr/0001-use-clean-architecture.md)
- [ADR-0002: Use Modular and Vertical-Slice Organization](docs/adr/0002-modular-vertical-slice-organization.md)
- [ADR-0003: Database Context and Migrations Strategy](docs/adr/0003-database-context-and-migrations.md)
- [ADR-0004: EventStoreDB and PostgreSQL Projections](docs/adr/0004-eventstoredb-and-postgresql-projections.md)
- [ADR-0005: Run Minutes Generation as a Python Worker](docs/adr/0005-python-minutes-worker.md)
