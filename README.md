# Warehouse

Offline-first, multi-tenant warehouse management SaaS.

Foundation through Phase 12 and **full production readiness** (VPS Compose + entitlements, stock counts, reports, Stripe, Flutter mobile scaffold) are implemented.

## Documents

| Topic | Document |
| --- | --- |
| Architecture and bounded contexts | [docs/architecture.md](docs/architecture.md) |
| Database | [docs/database.md](docs/database.md) |
| API | [docs/api.md](docs/api.md) |
| Authentication | [docs/authentication.md](docs/authentication.md) |
| Authorization | [docs/authorization.md](docs/authorization.md) |
| Offline-first | [docs/offline-first.md](docs/offline-first.md) |
| Synchronization | [docs/synchronization.md](docs/synchronization.md) |
| Conflict resolution | [docs/conflict-resolution.md](docs/conflict-resolution.md) |
| Deployment (VPS Compose + Caddy, Render Blueprint) | [docs/deployment.md](docs/deployment.md) |
| Stock counts | [docs/stock-counts.md](docs/stock-counts.md) |
| Reports | [docs/reports.md](docs/reports.md) |
| POS (Libya retail) | [docs/pos/README.md](docs/pos/README.md) · [docs/pos/inventory-integration-map.md](docs/pos/inventory-integration-map.md) |
| Troubleshooting | [docs/troubleshooting.md](docs/troubleshooting.md) |
| Business rules index | [docs/business-rules.md](docs/business-rules.md) |
| Product catalog | [docs/catalog.md](docs/catalog.md) |
| Inventory | [docs/inventory.md](docs/inventory.md) |
| Transfers | [docs/transfers.md](docs/transfers.md) |
| Purchasing | [docs/purchasing.md](docs/purchasing.md) |
| Sales | [docs/sales.md](docs/sales.md) |
| Web client | [docs/web.md](docs/web.md) · [clients/web/README.md](clients/web/README.md) |
| Platform admin | [docs/saas-platform.md](docs/saas-platform.md) · [clients/platform/README.md](clients/platform/README.md) |
| SaaS platform | [docs/saas-platform.md](docs/saas-platform.md) |

The product brief is [plan.md](plan.md).

## Prerequisites

- .NET SDK 8 (see `global.json`; prerelease SDKs are not used)
- Docker, when you want local PostgreSQL, Redis, and the API container

## Build and test

```text
dotnet build Warehouse.sln
dotnet test Warehouse.sln
```

## Run the API

Set `Database:ConnectionString`, `Redis:ConnectionString`, and `Jwt:SigningKey` / `JWT_SIGNING_KEY` (at least 32 characters). Apply migrations, then:

```text
dotnet run --project src/Warehouse.Api --launch-profile http
```

- `GET /health/live` — process liveness
- `GET /health/ready` — PostgreSQL + Redis readiness
- `POST /api/v1/auth/login` / `refresh` / `logout`, `GET /api/v1/auth/me`
- `POST /api/v1/devices/register`
- `GET /api/v1/roles`, `GET /api/v1/permissions`, `GET/POST /api/v1/users` (requires `users.manage`)

In Development, OpenAPI is at `/swagger`.

## Local stack

Copy `.env.example` to `.env`, then:

```text
docker compose up -d --build
```

Starts PostgreSQL, Redis, and the API on port **8080**. Production overlay + Caddy HTTPS: see [docs/deployment.md](docs/deployment.md).

## Remaining work

1. VPS restore drill (run `scripts/backup-postgres.sh` / restore once on target host)
2. Stripe test-mode PriceIds + webhook endpoint pointed at your DOMAIN
3. Expand Flutter workflows beyond the offline scaffold as field use requires

## Layout

```text
src/Warehouse.Domain
src/Warehouse.Application
src/Warehouse.Infrastructure
src/Warehouse.Shared
src/Warehouse.Api
tests/Warehouse.UnitTests
tests/Warehouse.IntegrationTests
tests/Warehouse.ArchitectureTests
clients/web          React + Vite tenant UI (Arabic RTL default, :5173)
clients/platform     React + Vite SaaS admin (:5174)
clients/mobile       Flutter offline scaffold (Drift + sync)
```

Dependency direction is enforced by the architecture tests. See [docs/architecture.md](docs/architecture.md).
