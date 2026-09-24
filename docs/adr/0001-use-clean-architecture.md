# ADR-0001: Use Clean Architecture

## Status

Accepted

## Context

DecisionPlanner is expected to evolve into a platform containing multiple business modules, such as Meeting and future decision-planning capabilities.

We want to keep business rules independent from frameworks, databases, HTTP, and other infrastructure concerns.

The architecture should also make the system easier to test, maintain, and evolve as new modules are introduced.

## Decision

DecisionPlanner will use Clean Architecture with four main projects:

- `DecisionPlanner.Domain`
- `DecisionPlanner.Application`
- `DecisionPlanner.Infrastructure`
- `DecisionPlanner.Api`

Dependencies will point inward toward the Domain layer.

### Dependency rules

- `Domain` has no dependencies on other DecisionPlanner projects.
- `Application` depends on `Domain`.
- `Infrastructure` depends on `Application` and `Domain`.
- `Api` depends on `Application` and `Infrastructure`.

The Domain layer contains business rules and domain models.

The Application layer contains use cases and application-level abstractions.

The Infrastructure layer contains implementations of infrastructure concerns such as persistence and external services.

The API layer exposes the application through HTTP and is responsible for transport-related concerns.

## Consequences

### Positive

- Business logic remains independent of infrastructure.
- Domain logic can be tested without a database or HTTP server.
- Infrastructure implementations can be replaced with limited impact on the core business logic.
- Clear boundaries make the system easier to evolve as new modules are introduced.
- Dependencies are explicit and easier to enforce.

### Negative

- The initial project structure is more complex than a single-project application.
- Some simple features may require multiple layers and files.
- Additional abstractions can introduce boilerplate if they are used without a clear need.

## Alternatives Considered

### Single project

A single ASP.NET Core project would be simpler initially but would provide weaker architectural boundaries as the system grows.

### N-Layer Architecture

Traditional Controllers → Services → Repositories layering was considered, but Clean Architecture provides stronger dependency direction and better separation between business rules and infrastructure.
