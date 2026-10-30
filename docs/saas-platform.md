# SaaS platform

Status: **implemented** (Phase 11) with **Stripe Checkout / Customer Portal** for plan subscriptions (P4).

## Concepts

| Concept | Ownership | Notes |
| --- | --- | --- |
| `PlatformOperator` | GLOBAL | Separate from tenant users; JWT claim `platform_operator=true`, no `tenant_id` |
| `Plan` | GLOBAL | Seeded `TRIAL` / `STANDARD` / `ENTERPRISE` with feature codes and soft limits |
| `TenantSubscription` | TENANT-OWNED | One row per tenant; status `Trial` / `Active` / `PastDue` / `Cancelled` |
| `IEntitlementService` | Application | Answers “is feature enabled?” for the ambient tenant; wired on business API groups |
| Tenant lifecycle | GLOBAL `tenants` | Suspend / resume, tax, offline policy, currency, timezone |
| Stripe | Config + webhooks | Checkout Session + Customer Portal; no card data stored locally |

`ExternalReference` stores the Stripe subscription id after Checkout completes. Entitlements stay usable for `Trial`, `Active`, and `PastDue`; `Cancelled` denies features. Suspended tenants also deny entitlements and fail normal login.

## Bootstrap (Development)

`appsettings.Development.json` → `Platform:BootstrapEmail` / `BootstrapPassword`. On API start, migrations run, plans are ensured, and the bootstrap operator is created if missing.

Default local operator:

- Email: `platform@warehouse.local`
- Password: `Platform-Pass-123!`

## Stripe billing

Config (`Stripe` section / env):

| Key | Purpose |
| --- | --- |
| `SecretKey` | Stripe secret key (test or live) |
| `WebhookSecret` | Signing secret for `/api/v1/billing/webhooks/stripe` |
| `PublishableKey` | Optional; for future client elements |
| `PriceIds:TRIAL` / `STANDARD` / `ENTERPRISE` | Stripe Price ids mapped to plan codes |
| `SuccessUrl` / `CancelUrl` | Checkout return URLs |

Tenant-owner flow:

1. `POST /api/v1/billing/checkout` with `{ "planCode": "STANDARD" }` → Stripe Checkout Session URL.
2. Stripe webhook `checkout.session.completed` activates the tenant subscription, assigns the plan, and sets `ExternalReference`.
3. `customer.subscription.updated` / `deleted` map to `Active` / `PastDue` / `Cancelled`.
4. `POST /api/v1/billing/portal` opens Customer Portal (requires existing `ExternalReference`).

Webhook handling is idempotent with respect to subscription state: repeated events re-apply the same plan/status/reference. Signature verification is mandatory (`Stripe-Signature`).

Web UI: tenant app `/billing`.

## API (platform)

```text
POST /api/v1/platform/auth/login

GET  /api/v1/platform/plans
GET  /api/v1/platform/tenants
POST /api/v1/platform/tenants
GET  /api/v1/platform/tenants/{id}
POST /api/v1/platform/tenants/{id}/suspend
POST /api/v1/platform/tenants/{id}/resume
PUT  /api/v1/platform/tenants/{id}/settings
GET  /api/v1/platform/tenants/{id}/subscription
PUT  /api/v1/platform/tenants/{id}/subscription
GET  /api/v1/platform/tenants/{id}/usage
GET  /api/v1/platform/tenants/{id}/readiness
```

All routes except login require the `PlatformOperator` authorization policy.

### Provision body

```json
{
  "tenantCode": "DEMO",
  "tenantName": "Demo Company",
  "currencyCode": "LYD",
  "timeZoneId": "Africa/Tripoli",
  "ownerEmail": "admin@demo.local",
  "ownerPassword": "Owner-Pass-123!",
  "ownerDisplayName": "Admin",
  "planCode": "TRIAL"
}
```

## Entitlements

Feature codes: `catalog`, `inventory`, `purchasing`, `sales`, `transfers`, `sync`, `reports`, `offline`.

API route groups call `RequireCatalog` / `RequireInventory` / … filters. Denied requests return **403** with `FEATURE_NOT_ENTITLED`.

## Web UI

- **Tenant app:** http://localhost:5173 (`clients/web`) — warehouse operations for tenant users.
- **Platform admin:** http://localhost:5174 (`clients/platform`) — SaaS operators only.

Arabic is default (RTL); English available. See [web.md](web.md) and `clients/platform/README.md`.
