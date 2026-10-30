# Troubleshooting

## `Application Control policy has blocked this file` (0x800711C7)

Windows Smart App Control (or another Application Control policy) is blocking unsigned DLLs built under a synced folder (commonly **OneDrive**).

This repo redirects `bin`/`obj` to `%LOCALAPPDATA%\WarehouseBuild\` via `Directory.Build.props`. After pulling that change:

```text
dotnet clean Warehouse.sln
dotnet run --project src/Warehouse.Api --launch-profile http
```

If it still fails:

1. **Windows Security → App & browser control → Smart App Control → Off** (requires restart; cannot be turned back on without reset on some builds), or
2. Clone/copy the repo to a **non-OneDrive** path such as `C:\dev\WareHouses`.

## The build uses a preview SDK

`global.json` requires the .NET 8 feature band (`8.0.100` with `latestFeature` roll-forward) and disallows prerelease SDKs. `dotnet --version` should report 8.0.x, not 10.0 preview. Install the .NET 8 SDK if the host only has a preview.

## `dotnet test` cannot find the solution

Architecture tests locate `Warehouse.sln` by walking upward from the ArchitectureTests source file (and optionally `WAREHOUSE_REPO_ROOT`). Local Windows builds may output binaries under `%LOCALAPPDATA%\WarehouseBuild\` (see `Directory.Build.props`); that does not affect repo-root discovery.

## `/health/live` fails or the browser shows a certificate warning

Trust the local HTTPS development certificate with `dotnet dev-certs https --trust` if the Development profile's HTTPS endpoint warns. Liveness itself does not require PostgreSQL or Redis. A failure there is the process, not the database.

`GET /health/ready` requires PostgreSQL and Redis (`Database:ConnectionString`, `Redis:ConnectionString`). A **503** with per-check statuses means a dependency is down — start `docker compose up -d postgres redis` (or the full stack).

## Docker Compose is up but the API has no data

Compose provides PostgreSQL, Redis, and (when using the `api` service) the API. Empty databases right after first start are normal until migrations/bootstrap run (Development applies migrations via platform bootstrap).

If port 5432, 6379, or 8080 is already taken, change `POSTGRES_PORT`, `REDIS_PORT`, or `API_PORT` in `.env`.

## Swagger is missing

Swagger is registered only for the Development environment. The test host uses `Testing` and does not expose it. Production must not turn Swagger on until there is an explicit decision.

## An error body contains no stack trace

Expected. Unhandled errors return `INTERNAL_ERROR`, a fixed message, and `correlationId`. Look up that id in the server logs. If the body contains SQL or a password, treat it as a defect.

## Auth returns 429 RATE_LIMITED

Expected under burst traffic. Login/refresh and platform login are limited to 20 requests per minute per IP. Wait for the window or change client IP in tests.
