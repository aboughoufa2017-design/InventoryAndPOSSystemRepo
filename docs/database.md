# Database strategy

Status: database foundation implemented. Initial EF Core migration: `InitialDatabaseFoundation`.

Shared PostgreSQL schema with `tenant_id` on tenant-owned tables. Catalog, inventory, transfers, purchasing, sales, sync change-log, and SaaS platform tables are implemented.

## Engine and change control

PostgreSQL 16 is the system of record. Schema changes ship as EF Core migrations under `src/Warehouse.Infrastructure/Persistence/Migrations`. Applied migrations are not edited or deleted to tidy history.

Local PostgreSQL and Redis are defined in `docker-compose.yml`. Connection string: `Database:ConnectionString` or `WAREHOUSE_DB_CONNECTION`.

## Naming convention

EF Core uses **EFCore.NamingConventions** (`UseSnakeCaseNamingConvention()`).

| C# | PostgreSQL |
| --- | --- |
| `TenantId` | `tenant_id` |
| `CreatedAtUtc` | `created_at_utc` |
| `PasswordHash` | `password_hash` |

Do not manually rename every column unless there is a documented exception.

## GLOBAL vs TENANT-OWNED

| Entity | Ownership | Notes |
| --- | --- | --- |
| `Tenant` | GLOBAL | Root company record and configuration |
| `Permission` | GLOBAL | Platform permission catalog (seeded) |
| `PlatformOperator` | GLOBAL | SaaS operator accounts (not tenant users) |
| `Plan` | GLOBAL | Commercial plans + feature codes / soft limits |
| `TenantSubscription` | TENANT-OWNED | One subscription per tenant; unique `tenant_id` |
| `User` | TENANT-OWNED | `tenant_id` required; unique `(tenant_id, email)` |
| `Role` | TENANT-OWNED | unique `(tenant_id, code)` |
| `RolePermission` | TENANT-OWNED | |
| `UserRole` | TENANT-OWNED | |
| `Warehouse` | TENANT-OWNED | unique `(tenant_id, code)` |
| `WarehouseLocation` | TENANT-OWNED | unique `(tenant_id, warehouse_id, code)` |
| `Device` | TENANT-OWNED | unique `(tenant_id, device_key)` |
| `ProcessedSyncOperation` | TENANT-OWNED | PK = `operation_id` (globally unique) |
| `AuditLog` | TENANT-SCOPED (nullable tenant) | Tenant actions set `tenant_id`; filtered by tenant context |

## Money and quantity

- Money columns (future commercial documents): `numeric(19,4)` via `MoneyPrecision`.
- Quantity columns (future inventory): `numeric(18,6)` via `QuantityPrecision`.
- Never `float` / `double`.
- Currency is per-tenant (`tenants.currency_code`, ISO 4217). Never hard-coded as a single product currency.
- Tax rate (when enabled): `numeric(9,6)` on `tenants`. Tax is **disabled by default**.

## Tenant isolation

- Shared schema `public`.
- Every tenant-owned table has `tenant_id uuid not null`.
- EF Core global query filters hide tenant rows unless `ITenantContext.TenantId` is set from authenticated server context.
- Client-supplied TenantId is never trusted as the filter source.
- Composite indexes lead with `tenant_id`.

## Warehouse locations and future stock

Every warehouse must have at least:

| Code | Kind | Role |
| --- | --- | --- |
| `GENERAL` | `System` | Default available location |
| `IN_TRANSIT` | `InTransit` | Dedicated in-transit bucket; never sellable/pickable |

Additional locations (`A-01`, `RECEIVING`, `DAMAGED`, …) use kind `Normal` or `System` as appropriate.

**Future StockBalance grain (locked — not implemented):**

| Column | Type / rule |
| --- | --- |
| Id | UUID |
| TenantId | NOT NULL |
| ProductId | NOT NULL |
| WarehouseId | NOT NULL |
| LocationId | **NOT NULL** |
| OnHand | `numeric(18,6)` |
| Reserved | `numeric(18,6)` |

**Available = OnHand - Reserved** (calculated in domain/application; never a persisted column).

In-transit stock is **OnHand on the `IN_TRANSIT` location**. Reserved on that location is normally zero. There is no separate in-transit balance concept.

**Future StockMovement** is the authoritative ledger. Optional nullable `TransferLineId` plus `SourceDocumentType` / `SourceDocumentId` for traceability. StockBalance is a projection.

## Sync idempotency

Table `processed_sync_operations`:

- Primary key / unique: `id` (= client `OperationId`, uuid)
- Indexes: `(tenant_id, created_at_utc)`, `(tenant_id, device_id)`, `(tenant_id, entity_type, entity_id)`
- No partitioning in v1
- Result payload stored as `jsonb`

## Devices and offline policy

`devices` stores registration, revocation status, app version, last successful sync, and last successful online authentication.

Tenant offline policy fields on `tenants`:

- `max_offline_days` — default application value **7** (`OfflineSessionPolicy.DefaultMaxOfflineDays`); tenant-configurable; column remains nullable for explicit override semantics
- `require_online_revalidation`
- `is_suspended`

Security flags `DeviceRevoked` / `UserDisabled` / `TenantSuspended` map to `devices.status`, `users.is_disabled`, and `tenants.is_suspended`.

## Auth tables (Phase 2)

`refresh_tokens` stores only SHA-256 token hashes, with unique `token_hash`, `(tenant_id, user_id)`, and `family_id` indexes. See [authentication.md](authentication.md).

## Catalog tables (Phase 3)

| Table | Notes |
| --- | --- |
| `units_of_measure` | Tenant-owned; unique `(tenant_id, code)`; system units seeded on provision |
| `product_categories` | Tenant-owned; optional parent; unique `(tenant_id, code)` |
| `products` | Tenant-owned SKU; money `numeric(19,4)`; stock thresholds `numeric(18,6)` |
| `product_barcodes` | Multiple per product; unique `(tenant_id, barcode)` |

Tax: product `is_taxable` + tenant `tax_enabled`/`tax_rate`. When tenant tax is disabled, applicable rate is always 0.

## Inventory tables (Phase 4)

| Table | Notes |
| --- | --- |
| `stock_balances` | Unique `(tenant_id, product_id, warehouse_id, location_id)`; OnHand/Reserved; Available not stored |
| `stock_movements` | Immutable ledger; nullable `transfer_line_id`, source document fields |
| `stock_adjustments` / `stock_adjustment_lines` | OpeningBalance / Correction; post creates movements |

## Transfer tables (Phase 5)

| Table | Notes |
| --- | --- |
| `transfers` | Source/destination warehouse+location; status lifecycle; optional dispatch_operation_id |
| `transfer_lines` | Requested/received quantities; in-transit calculated |
| `transfer_operations` | Idempotency store (PK = client operation id) |

## Purchasing tables (Phase 6)

| Table | Notes |
| --- | --- |
| `suppliers` | Tenant-owned; unique `(tenant_id, code)` |
| `purchase_orders` | Module lifecycle + `PaymentStatus`; money totals `numeric(19,4)` |
| `purchase_order_lines` | Ordered/received/returned qty; unit price + tax |
| `purchase_receipts` / `purchase_receipt_lines` | Receiving posts StockMovements |
| `purchase_returns` / `purchase_return_lines` | Returns post compensating movements |
| `purchase_operations` | Idempotency store (PK = client operation id) |

## Sales tables (Phase 7)

| Table | Notes |
| --- | --- |
| `customers` | Tenant-owned; unique `(tenant_id, code)` |
| `sales_orders` | Module lifecycle + `PaymentStatus`; money totals `numeric(19,4)` |
| `sales_order_lines` | Ordered/shipped/returned qty; unit price + tax |
| `sales_shipments` / `sales_shipment_lines` | Shipping posts StockMovements out |
| `sales_returns` / `sales_return_lines` | Returns post compensating movements in |
| `sales_operations` | Idempotency store (PK = client operation id) |

## Sync tables (Phase 8)

| Table | Notes |
| --- | --- |
| `processed_sync_operations` | Push idempotency (foundation; PK = operation id) |
| `tenant_change_counters` | Monotonic change version per tenant |
| `change_log` | Pull feed; unique `(tenant_id, change_version)`; Upsert/Deactivate |

## SaaS platform tables (Phase 11)

| Table | Notes |
| --- | --- |
| `platform_operators` | GLOBAL; unique email; password hash |
| `plans` | GLOBAL; unique code; `feature_codes` text array; soft limits nullable |
| `tenant_subscriptions` | TENANT-OWNED; unique `tenant_id`; status enum; optional `external_reference` |

Migration: `AddSaaSPlatform`. Default plans (`TRIAL` / `STANDARD` / `ENTERPRISE`) are ensured at API startup.

## Future tables (not yet implemented)

None required for core warehouse modules in v1 beyond operational retention of `change_log`.

## Audit log

`audit_logs` is append-only. TenantId may be null for platform-level events. **No automatic retention/deletion in v1.** Retention is a future operational concern.

## Soft delete strategy

No global `is_deleted`. Master data uses `is_active` / `is_disabled` / `is_suspended`. Posted transactional history (when added) must not be deleted.

## Document lifecycle and payment status (schema guidance)

Future commercial documents use:

- Lifecycle: `Draft`, `Approved`, `Posted`, `Cancelled` (`DocumentLifecycleStatus`)
- Payment: `Unpaid`, `PartiallyPaid`, `Paid` (`PaymentStatus`)
- No general ledger tables in v1

Inventory changes only when documents are posted.

## Important indexes (foundation)

| Table | Index |
| --- | --- |
| tenants | unique `code`; `name` |
| permissions | unique `code` |
| users | unique `(tenant_id, email)`; `(tenant_id, is_active)` |
| roles | unique `(tenant_id, code)` |
| warehouses | unique `(tenant_id, code)`; `(tenant_id, name)`; `(tenant_id, is_active)` |
| warehouse_locations | unique `(tenant_id, warehouse_id, code)`; `(tenant_id, warehouse_id, kind)` |
| devices | unique `(tenant_id, device_key)`; `(tenant_id, status)` |
| processed_sync_operations | PK `id`; `(tenant_id, created_at_utc)` |
| audit_logs | `(tenant_id, occurred_at_utc)`; `(tenant_id, entity_type, entity_id)` |

## Migrations

```text
dotnet ef migrations add <Name> --project src/Warehouse.Infrastructure --startup-project src/Warehouse.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Warehouse.Infrastructure --startup-project src/Warehouse.Api
```

Initial migration: `20260926141912_InitialDatabaseFoundation`.

## What is deliberately absent

Product, StockBalance, StockMovement, Transfer, TransferLine, Purchase, Sale tables. They will be added in later phases using the location and UUID rules above.
