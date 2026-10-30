# Deployment

Status: **VPS production path implemented** via Docker Compose + Caddy HTTPS. Full CD to a remote host remains manual (`ssh` + `docker compose`). Exercise backup/restore once on the target VPS before claiming commercial RPO/RTO.

A **Render Blueprint** path is also available (`render.yaml` at the repo root) for API + managed Postgres/Redis + both Static Sites (`clients/web`, `clients/platform`). See [Render](#render) below.

## Environments

`Development`, `Testing`, `Staging`, and `Production` are separate configurations. Secrets differ by environment. `appsettings.json` / `appsettings.Production.json` hold non-secret defaults only. Connection strings, JWT signing keys, and database passwords are environment variables (or a secret manager in production).

`.env.example` lists the local placeholders. Copy it to `.env` for Docker Compose. `.env` is gitignored.

| Variable | Purpose |
| --- | --- |
| `Database__ConnectionString` / `WAREHOUSE_DB_CONNECTION` | PostgreSQL |
| `Redis__ConnectionString` | Redis (required for `/health/ready`) |
| `Jwt__SigningKey` / `JWT_SIGNING_KEY` | Access-token signing (≥ 32 chars; **required** in Production — API refuses to start otherwise) |
| `Cors__Origins__N` / `WEB_ORIGIN` / `PLATFORM_ORIGIN` | Production CORS allowlist |
| `DOMAIN` | Public hostname for Caddy TLS |
| `Stripe__SecretKey` / `Stripe__WebhookSecret` / `Stripe__PriceIds__*` | Optional until billing is enabled |
| `Platform__BootstrapEmail` / `Platform__BootstrapPassword` | Dev/bootstrap platform operator |
| `OTEL_EXPORTER_CONSOLE` | When `true`, dump OpenTelemetry to stdout |

On **Production**, missing `Database__ConnectionString`, `Redis__ConnectionString`, or a JWT signing key shorter than 32 characters fails fast at startup (no dummy JWT fallback).

## Local stack

```text
docker compose up -d --build
```

Starts PostgreSQL 16, Redis 7, and the API image (port **8080** by default).

Or run the API on the host against Compose infra only:

```text
docker compose up -d postgres redis
dotnet run --project src/Warehouse.Api --launch-profile http
```

Host profile: `src/Warehouse.Api/Properties/launchSettings.json` (http → 5011). In Development, OpenAPI is at `/swagger`.

## Production VPS (Compose + Caddy)

```text
docker compose -f docker-compose.yml -f docker-compose.prod.yml --profile migrate run --rm migrate
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

- **Caddy** terminates HTTPS on 80/443 (`deploy/Caddyfile`); Postgres/Redis/API ports are not published publicly.
- **API** runs with `ASPNETCORE_ENVIRONMENT=Production`; auto-migrate is **disabled** in Production — use the `migrate` profile (or `dotnet Warehouse.Api.dll --migrate-only`) once before traffic.
- Security headers middleware sets HSTS (when behind TLS), `X-Content-Type-Options`, `X-Frame-Options`, and `Referrer-Policy`.
- Production CORS is config-driven (`Cors:Origins`), not localhost-only.

Validate compose merge locally (no containers):

```text
docker compose -f docker-compose.yml -f docker-compose.prod.yml config
```

## Health

| Route | Behavior |
| --- | --- |
| `GET /health/live` | Process up. Body `{ "status": "Healthy" }`. No dependency details. |
| `GET /health/ready` | PostgreSQL + Redis. Body includes per-check statuses. **200** when healthy, **503** otherwise. Exception messages are not published. |

Orchestrators should route traffic only when ready returns 200.

## Rate limiting

- Global fixed window per client IP (excludes `/health` and `/swagger`).
- Stricter policy `auth` on tenant login/refresh and platform login (20 requests / minute / IP).
- In-memory store is acceptable for a single VPS; Redis-backed counters can wait until multi-node.
- Rejected requests return `429` with `RATE_LIMITED` and a correlation id.

## Observability

- **Serilog** compact JSON to console, enriched with `CorrelationId` via log scope (tokens redacted via destructuring policies where configured).
- **OpenTelemetry** ASP.NET Core + HTTP + runtime instrumentation. Console exporter only when `OTEL_EXPORTER_CONSOLE=true`.

## Migrations (deploy procedure)

1. Take an application-consistent PostgreSQL backup (or rely on continuous WAL if configured).
2. Run migrations **once** before new application replicas serve traffic:

```text
# Host tooling
dotnet ef database update --project src/Warehouse.Infrastructure --startup-project src/Warehouse.Api

# Or Compose one-shot (Production overlay)
docker compose -f docker-compose.yml -f docker-compose.prod.yml --profile migrate run --rm migrate
```

Scripts: `scripts/migrate.sh`. Do **not** let every replica migrate concurrently without a lock.

3. In Development, `PlatformBootstrap.EnsureAsync` applies pending migrations on API start. Production skips auto-migrate.
4. Roll forward only; do not invent down-migrations in production without a tested rollback plan.

## Backups and recovery

Scripts: `scripts/backup-postgres.sh`, `scripts/restore-postgres.sh`. Example cron (daily 02:15 UTC):

```text
15 2 * * * /opt/warehouse/scripts/backup-postgres.sh >> /var/log/warehouse-backup.log 2>&1
```

### Assumptions (until a restore test is recorded on the VPS)

| Metric | Assumption |
| --- | --- |
| RPO | Last successful dump (typically ≤ 24h with daily cron; tighten with WAL/PITR if needed) |
| RTO | Time to provision empty Postgres + `pg_restore` + migrate check + bring compose up (often under 1 hour for a small DB) |

Do not treat these as contractual until at least one restore drill succeeds.

### PostgreSQL (system of record)

```text
# Logical dump (script wraps pg_dump -Fc)
./scripts/backup-postgres.sh

# Restore into an empty database
./scripts/restore-postgres.sh warehouse-YYYYMMDD.dump
```

### Redis

Ephemeral. Losing Redis must not lose posted inventory. After Redis loss, restart API; readiness fails until Redis returns.

## Production shape

- HTTPS at the edge (Caddy).
- Secrets from the host or a secret manager — never baked into the image.
- Migrate once, then scale API replicas (same image; no auto-migrate).
- Monitor `/health/ready`, auth `429` rates, and Serilog errors by `CorrelationId`.

## Render

Blueprint file: [`render.yaml`](../render.yaml). It creates:

| Resource | Name | Role |
| --- | --- | --- |
| Web Service (Docker) | `warehouse-api` | `Warehouse.Api` from the root [Dockerfile](../Dockerfile) (port **8080**) |
| Postgres | `warehouse-db` | System of record |
| Key Value | `warehouse-redis` | Required for Production startup + `/health/ready` |
| Static Site | `warehouse-web` | Tenant UI (`clients/web` → `dist`) |
| Static Site | `warehouse-platform` | Platform admin (`clients/platform` → `dist`) |

VPS Compose + Caddy remains the primary self-hosted path; Render does not replace it.

### Apply the Blueprint

1. Push `main` to GitHub (`aboughoufa2017-design/InventoryAndPOSSystemRepo` or your fork).
2. In [Render](https://dashboard.render.com): **New** → **Blueprint** → select the repo / branch with `render.yaml`.
3. When prompted (`sync: false` secrets), set:
   - `JWT_SIGNING_KEY` — random secret, **≥ 32** characters
   - `Cors__Origins__0` — temporary placeholder is fine (e.g. `https://example.com`); update after Static Sites get URLs
   - `Cors__Origins__1` — same for the platform site
4. Create the Blueprint and wait for Postgres, Redis, API, and both Static Sites to finish.

### After first deploy

1. Copy the public URLs for `warehouse-web` and `warehouse-platform` (e.g. `https://warehouse-web-xxxx.onrender.com`).
2. On **warehouse-api** → Environment, set:
   - `Cors__Origins__0` = web Static Site origin (no trailing slash)
   - `Cors__Origins__1` = platform Static Site origin
3. Redeploy the API so CORS takes effect.
4. Confirm Static Sites baked `VITE_API_BASE_URL` from the API’s `RENDER_EXTERNAL_URL` (Blueprint `fromService`). If the API URL changed after the first frontend build, **Clear build cache & deploy** on each Static Site.
5. Health checks:
   - `GET https://<api>/health/live` → 200
   - `GET https://<api>/health/ready` → 200 (Postgres + Redis)

### Migrations on Render

Production does **not** auto-migrate. The Blueprint sets:

```text
preDeployCommand: dotnet Warehouse.Api.dll --migrate-only
```

**Free** Web Services do not run `preDeployCommand`. On the free plan, run migrate once from the API service **Shell** (or upgrade the API to `0.5c-512mb` / higher so pre-deploy runs automatically):

```text
dotnet Warehouse.Api.dll --migrate-only
```

Connection strings are already injected as `Database__ConnectionString` / `WAREHOUSE_DB_CONNECTION` and `Redis__ConnectionString` from the Blueprint links.

### Frontend build notes

- Build command: `npm ci && npm run build` (lockfiles present under each client).
- Publish directory: `dist`.
- SPA fallback: Blueprint rewrite `/*` → `/index.html` (React Router `BrowserRouter`).
- `VITE_API_BASE_URL` is build-time only; it must be the public API origin with **no** trailing slash.

### Plans and cold starts

Default Blueprint plans are **`free`** where available (API, Postgres, Redis). Expect:

- API **cold starts** after idle on free Web Services
- Free Redis has **no persistence** (acceptable — Redis is not the system of record)
- Free Postgres limits; upgrade plans in the Dashboard or edit `plan:` in `render.yaml` when you outgrow them

Static Sites are CDN-backed and do not sleep.

### Optional: custom domains

Attach custom domains in the Render Dashboard for API + both Static Sites, then update `Cors__Origins__*` and rebuild the frontends if the API public URL changes.

## CI

`.github/workflows/ci.yml` restores, builds, and tests on .NET 8, builds the API image, validates prod compose config, then runs a Compose smoke that waits for `/health/ready`. It does not deploy to staging/production (VPS or Render).
