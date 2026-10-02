# Local development

This guide starts the local databases, applies PostgreSQL migrations, and runs the .NET API.

## Prerequisites

- Docker Desktop (or another Docker Compose implementation)
- .NET 10 SDK
- EF Core CLI (`dotnet-ef`) version 10

Check the installed SDK and EF CLI:

```bash
dotnet --version
dotnet ef --version
```

If the EF CLI is not installed, install the .NET 10 tool:

```bash
dotnet tool install --global dotnet-ef --version '10.*'
```

If it is already installed with an older major version, update it:

```bash
dotnet tool update --global dotnet-ef --version '10.*'
```

## Start the databases

Run these commands from the repository root:

```bash
docker compose up -d
docker compose ps
```

The default Compose services start PostgreSQL and KurrentDB. PostgreSQL is available on `localhost:5433`; KurrentDB is available on `localhost:2113`. The Python FastAPI service and minutes worker start only when the optional `minutes` Compose profile is selected; see [ADR-0005](adr/0005-python-minutes-worker.md).

## Apply PostgreSQL migrations

Apply all pending migrations to the local `decision_planner` database:

```bash
dotnet ef database update \
  --project src/DecisionPlanner.Migrations \
  --startup-project src/DecisionPlanner.Migrations
```

The migrations project uses a design-time `DbContext` factory configured for the local Compose database (`localhost:5433`, database `decision_planner`, user/password `postgres`). On a fresh database, this creates the schema and EF migration history. Running the command again applies only migrations that have not yet been applied.

To create a migration after changing the EF model or configuration, use:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/DecisionPlanner.Migrations \
  --startup-project src/DecisionPlanner.Migrations \
  --output-dir Migrations
```

Then apply it with `dotnet ef database update` as shown above. Commit the generated migration files; do not edit generated build files under `bin/` or `obj/`.

## Run the API

Once the databases are running and migrations are applied, configure the API with the HTTP launch profile. For minutes generation, store the same callback secret used for `MINUTES_SERVICE_API_KEY` in the Python root `.env`. If User Secrets has not been initialized for the API project, run this once:

```bash
dotnet user-secrets init --project src/DecisionPlanner.Api
```

Set or update the secret:

```bash
dotnet user-secrets set "MinutesService:ApiKey" "<same-value-as-MINUTES_SERVICE_API_KEY>" \
  --project src/DecisionPlanner.Api
```

Then run the API:

```bash
dotnet run --project src/DecisionPlanner.Api --launch-profile http
```

The API listens at `http://localhost:5189`. In Development, Scalar is available at `http://localhost:5189/scalar/v1`, and the OpenAPI document is at `http://localhost:5189/openapi/v1.json`. The API connects to KurrentDB at `localhost:2113` and PostgreSQL at `localhost:5433` using the checked-in Development configuration.

## Run the Python minutes service

The Python service has two separate processes: a FastAPI service that exposes health endpoints and a worker that consumes completed-meeting events and generates drafts. Both can run through the optional `minutes` Compose profile; starting FastAPI alone does not consume events. See the [minutes service README](../services/minutes/README.md) for its internal design.

The worker needs KurrentDB and the .NET API running, a Gemini API key, and a service key shared with the .NET API. Set `GEMINI_API_KEY` and `MINUTES_SERVICE_API_KEY` in the root `.env`. Set the same service-key value as `MinutesService:ApiKey` in .NET User Secrets as described above. Keep the `.env` file untracked.

### Start FastAPI and the worker with Compose

From the repository root, create the root `.env` file if you have not already:

```bash
cp services/minutes/.env.example .env
```

Set `GEMINI_API_KEY` and `MINUTES_SERVICE_API_KEY` in `.env`, then start both Python services:

```bash
docker compose --profile minutes up -d --build minutes-api minutes-worker
```

FastAPI is available on `http://localhost:8000`. Check that it is running:

```bash
curl -i http://localhost:8000/health
```

To make one small, real generation request and verify Gemini connectivity, call this manually:

```bash
curl -i http://localhost:8000/health/ai-provider
```

This AI-provider check uses Gemini and consumes a request, so do not use it as a frequent automatic liveness probe. Inspect the worker logs with:

```bash
docker compose logs -f minutes-worker
```

FastAPI and the worker run in separate containers. Compose binds FastAPI to localhost only. The worker connects to KurrentDB by its Compose service name and calls the .NET API on the host at `http://host.docker.internal:5189`. Start the worker before completing meetings; a newly created subscription starts at the end of the event log.

### Run Python directly on the host (alternative)

Requires Python 3.11 or newer. From `services/minutes`, create a virtual environment and local settings:

```bash
cd services/minutes
python3.11 -m venv .venv
source .venv/bin/activate
python -m pip install -e .
cp .env.example .env
```

Set `GEMINI_API_KEY` and `MINUTES_SERVICE_API_KEY` in `services/minutes/.env`. Run either process in its own terminal with the virtual environment activated:

```bash
python -m minutes_service.entrypoints.worker
```

```bash
uvicorn minutes_service.entrypoints.api:app --reload
```

The FastAPI service listens on `http://localhost:8000`; the worker and API remain separate processes.

Stop the API with `Ctrl+C`. Stop the Compose services with:

```bash
docker compose down
```

This leaves the named database volumes in place, so local data remains for the next startup.
