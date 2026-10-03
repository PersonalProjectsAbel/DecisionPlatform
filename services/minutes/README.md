# DecisionPlanner Minutes Service

Initial service foundation for AI-assisted meeting minutes. The application-facing AI contract is provider-neutral; the composition container selects the Gemini adapter.

```text
Application code -> AIProvider.generate(system_prompt, user_prompt)
                              ^
                              |
Composition container -> GeminiProvider -> Google GenAI SDK
```

## Local setup

Requires Python 3.11 or newer. From this directory:

```sh
python -m venv .venv
source .venv/bin/activate
python -m pip install -e .
cp .env.example .env
# Set GEMINI_API_KEY in .env before making AI calls.
uvicorn minutes_service.entrypoints.api:app --reload
```

Health endpoint: `GET http://localhost:8000/health`.

To verify that the configured AI provider is reachable, call `GET http://localhost:8000/health/ai-provider`. This makes one small generation request through the configured provider; call it manually rather than as a frequent liveness probe.

For Compose development, the FastAPI service and event worker run in separate containers under the same profile:

```sh
docker compose --profile minutes up -d --build minutes-api minutes-worker
```

FastAPI is published on localhost port 8000. The worker remains a separate event-consuming process.

Run the event worker in a separate process with `python -m minutes_service.entrypoints.worker`.

`GEMINI_MODEL` defaults in `.env.example` to `gemini-3.8-flash` and can be changed through environment configuration. The API key is read from `GEMINI_API_KEY`; it is not stored in source control.

## Generated minutes format

The worker submits the AI-generated minutes draft as Markdown. Drafts use these sections so readers can quickly find the meeting summary, discussion, proposals, decisions, and follow-up work:

```markdown
## Summary

A concise summary of the meeting's purpose and outcome.

## Topics and Discussion

### General

- Discussion points, attributed to participants when useful.

## Proposals

- Proposals discussed, or "None recorded."

## Decisions

- Decisions made, or "None recorded."

## Action Items

- Follow-up tasks with an owner when one was identified, or "None recorded."
```

Include only information supported by the recorded meeting history. If a category has no recorded content, say "None recorded." The draft remains a draft for review; it does not itself record a decision or create an action in the domain model.

## Current step

The service includes a persistent KurrentDB worker filtered to `meeting.completed.v1`. It reads the full Meeting stream, builds context from the recorded topics/proposals/discussion, generates a Markdown draft via `AIProvider`, and submits it to the .NET API. The worker acknowledges only after the API accepts the draft; failed deliveries are retried. The .NET API must be running at `DECISION_PLANNER_API_URL` and configured with the same `MINUTES_SERVICE_API_KEY`. The subscription starts at the end of the event log when first created, so it will process future completions without automatically sending historical meetings to Gemini. Start the worker before completing meetings; existing completed meetings require an intentional backfill. Subscription creation requires KurrentDB admin privileges. The result contract is `POST /api/internal/meetings/{meetingId}/minutes-draft`, carrying the source `completedEventId`, draft `content`, and `generatedAt`; .NET appends `meeting.minutes-draft-generated.v1`. A repeated source completion ID is idempotent.

To enable this callback in the .NET API, set `MinutesService__ApiKey` to the same secret used for `MINUTES_SERVICE_API_KEY`. The worker can run on the host with `python -m minutes_service.entrypoints.worker` or as the opt-in Compose `minutes-worker` service (`docker compose --profile minutes up -d --build minutes-worker`). The Compose worker connects to the existing KurrentDB container and calls the API on the host at port 5189. Set `GEMINI_API_KEY` and `MINUTES_SERVICE_API_KEY` in the environment or root `.env`; set `MinutesService__ApiKey` to the same value when starting the .NET API. The local KurrentDB is configured in insecure mode, so subscription creation is allowed; production credentials should grant the required subscription permissions.
