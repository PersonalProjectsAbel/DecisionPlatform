# ADR-0003: Database Context and Migrations Strategy

## Status

Accepted

## Context

DecisionPlanner uses Clean Architecture with separate projects for Domain, Application, Infrastructure, and API.

The application uses PostgreSQL as its persistence store and Entity Framework Core as the ORM.

We need to define where the EF Core `DbContext` and database migrations belong while keeping the Domain and Application layers independent from persistence technology.

The database schema will evolve together with the application's business modules, so database changes need to be versioned and reproducible across development, testing, and deployment environments.

## Decision

### DbContext

The EF Core `DbContext` will live in the `DecisionPlanner.Infrastructure` project.

```text
DecisionPlanner.Infrastructure/
└── Persistence/
    ├── DecisionPlannerDbContext.cs
    └── Configurations/
```

The Domain and Application projects will not reference Entity Framework Core or PostgreSQL.

Infrastructure is responsible for implementing persistence concerns.

### Migrations

EF Core migrations will live in a dedicated `DecisionPlanner.Migrations` project.

```text
DecisionPlanner.Migrations/
└── Migrations/
    ├── <timestamp>_InitialCreate.cs
    └── ...
```

The Migrations project references Infrastructure in order to access the `DecisionPlannerDbContext`.

The Infrastructure project will configure EF Core to use `DecisionPlanner.Migrations` as its migrations assembly.

```csharp
options.UseNpgsql(
    connectionString,
    npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(
            "DecisionPlanner.Migrations");
    });
```

### Migration Execution

Database migrations will be created and applied using the EF Core CLI.

For example:

```bash
dotnet ef migrations add InitialCreate \
  --project src/DecisionPlanner.Migrations \
  --startup-project src/DecisionPlanner.Api
```

Database updates will be applied with:

```bash
dotnet ef database update \
  --project src/DecisionPlanner.Migrations \
  --startup-project src/DecisionPlanner.Api
```

The migration files will be committed to source control.

## Project Responsibilities

| Project                          | Responsibility                                    |
| -------------------------------- | ------------------------------------------------- |
| `DecisionPlanner.Domain`         | Business rules and domain model                   |
| `DecisionPlanner.Application`    | Use cases and application abstractions            |
| `DecisionPlanner.Infrastructure` | EF Core, `DbContext`, persistence implementations |
| `DecisionPlanner.Migrations`     | EF Core migration history and schema evolution    |
| `DecisionPlanner.Api`            | HTTP API and application composition              |

## Consequences

### Positive

- The Domain remains independent of EF Core and PostgreSQL.
- Persistence concerns remain inside Infrastructure.
- Database schema changes are version-controlled.
- Migrations can be executed independently from the API application.
- The migration project provides a clear boundary for database deployment concerns.
- The strategy works well as additional modules and tables are introduced.

### Negative

- An additional project increases the initial solution complexity.
- Developers need to specify the migration and startup projects when using the EF CLI.
- The Migrations project has a dependency on Infrastructure.

## Alternatives Considered

### Migrations inside Infrastructure

This would reduce the number of projects but would combine persistence implementation with database schema history.

A dedicated Migrations project provides a clearer separation between runtime persistence and database migration execution.

### Migrations inside the API project

This would couple database schema management to the HTTP application.

The API should remain responsible for exposing the application rather than owning database deployment concerns.

### Database-first schema management

Managing SQL scripts independently from EF Core was considered, but EF Core migrations provide a convenient way to version schema changes alongside the application's model.

## Future Considerations

If the deployment architecture changes, migrations may eventually be executed as part of a dedicated deployment step, CI/CD pipeline, or database migration container.

The decision to use a dedicated migrations project does not require migrations to be executed by the API at application startup.
