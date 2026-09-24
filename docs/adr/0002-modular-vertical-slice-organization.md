# ADR-0002: Use Modular and Vertical-Slice Organization Within Clean Architecture

## Status

Accepted

## Context

DecisionPlanner uses Clean Architecture at the project level, with separate projects for Domain, Application, Infrastructure, and API.

As the platform grows, it will contain multiple business capabilities such as Meeting, Decision, Participant, and Planning.

A purely layer-oriented folder structure could cause related functionality to become spread across large shared folders. For example:

```text
Application/
├── Commands/
├── Queries/
├── Handlers/
└── Services/
```

This makes it harder to identify the complete functionality belonging to a specific business capability.

We want to preserve the architectural boundaries between projects while keeping related business functionality close together.

## Decision

DecisionPlanner will use **Clean Architecture at the project level** and **modular/vertical-slice organization inside each project**.

Business capabilities will be organized around modules rather than technical concepts.

For example:

```text
DecisionPlanner.Domain/
└── Meeting/
    ├── Aggregates/
    ├── Entities/
    ├── ValueObjects/
    └── Events/
```

```text
DecisionPlanner.Application/
└── Meeting/
    ├── CreateMeeting/
    ├── AddParticipant/
    └── GetMeeting/
```

```text
DecisionPlanner.Infrastructure/
└── Meeting/
    ├── Persistence/
    └── Repositories/
```

The API will expose module-specific endpoints:

```text
DecisionPlanner.Api/
└── Meeting/
    └── MeetingController.cs
```

The exact internal structure of each module may evolve according to its complexity. We will avoid creating abstractions or folders that do not provide a clear architectural benefit.

## Module Boundaries

Each module represents a distinct business capability.

For example:

```text
Meeting
Decision
Participant
Planning
```

Modules should minimize direct dependencies on each other.

When communication between modules is required, it should preferably occur through explicit contracts, application interfaces, or domain/integration events rather than accessing another module's internal implementation directly.

## Vertical Slices

Within the Application layer, functionality will preferably be organized by use case.

For example:

```text
Meeting/
├── CreateMeeting/
│   ├── CreateMeetingCommand.cs
│   └── CreateMeetingHandler.cs
│
├── AddParticipant/
│   ├── AddParticipantCommand.cs
│   └── AddParticipantHandler.cs
│
└── GetMeeting/
    ├── GetMeetingQuery.cs
    └── GetMeetingHandler.cs
```

A use case should keep its related request, handler, validation, and other use-case-specific components together when appropriate.

Shared abstractions will only be introduced when they represent a genuine shared concept.

## API Transport Contracts

HTTP request and response DTOs belong to the API project and are kept inside the relevant feature slice.

For example:

```text
DecisionPlanner.Api/
└── Meeting/
    └── AddParticipant/
        └── AddParticipantRequest.cs
```

These transport contracts are mapped to Application commands or queries. The Application layer does not depend on HTTP-specific request or response types.

## Consequences

### Positive

- Business capabilities remain easy to locate.
- Related functionality stays together.
- Module boundaries become explicit.
- Vertical slices reduce unnecessary shared abstractions.
- The architecture can evolve toward independently deployable services if a module eventually requires it.
- The system remains a modular monolith initially without prematurely introducing microservices.

### Negative

- Developers must understand both the Clean Architecture layers and module boundaries.
- Some concepts may appear in multiple modules when they genuinely belong to different bounded contexts.
- Determining when something should become shared requires architectural judgment.
- There may be some duplication between modules, which is acceptable when it preserves boundaries.

## Alternatives Considered

### Pure layer-based organization

```text
Application/
├── Commands/
├── Queries/
├── Handlers/
└── Services/
```

Rejected because functionality belonging to the same business capability becomes distributed across technical folders.

### Feature folders without Clean Architecture

Organizing everything by feature was considered, but it does not provide sufficient separation between domain, application, infrastructure, and API concerns for this system.

### Microservices per module

Creating a separate deployable service for every module was considered but rejected at this stage.

DecisionPlanner will initially remain a modular monolith. Modules can be extracted into services later if there is a concrete architectural or operational reason to do so.
