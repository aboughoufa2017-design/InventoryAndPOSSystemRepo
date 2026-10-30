# General-Purpose POS SaaS for Libya

## MVP Architecture and Implementation Plan

**Version:** 1.0\
**Date:** 2026-10-26\
**Status:** Draft implementation plan\
**Technology:** Flutter, ASP.NET Core, PostgreSQL\
**Important project context:** An inventory system already exists in the
current project. The POS must integrate with and extend that existing
inventory system rather than rebuild it.

------------------------------------------------------------------------

## 1. Executive Summary

Build a multi-tenant, offline-first point-of-sale SaaS for shops in
Libya. The product will support:

-   Hybrid checkout: Flutter mobile applications and optional PC/browser
    checkout.
-   Platforms: Android, iOS, and web.
-   Initial business profiles: general retail, groceries, wholesale, and
    restaurants.
-   Offline checkout with later synchronization.
-   Arabic RTL and English interfaces.
-   Libyan dinar (LYD) as the default currency, with currency
    configuration designed to avoid hard-coding.
-   A shared sales, payment, and inventory foundation with configurable
    workflows for different business types.

The existing inventory system is a key asset. The first engineering task
is to inspect its current entities, services, database schema, APIs,
stock-movement logic, and authentication/tenant model. Reuse compatible
components, map gaps, and introduce adapters or migrations where
necessary. Do not create a second product catalog or a parallel stock
ledger unless a documented architectural reason requires it.

**Primary MVP goal:** A shop can complete sales on a supported device
without internet, retain those sales safely, and synchronize them later
without duplicate transactions or silent data loss.

## 2. Product Scope

### 2.1 Shared core

The shared platform should provide:

-   Business/tenant onboarding and configuration.
-   Branches and warehouses.
-   Users, roles, permissions, and registered POS devices.
-   Product catalog and barcode lookup, preferably reusing the existing
    inventory system.
-   Price lists, discounts, and customer-specific prices.
-   Checkout, sale lines, payments, receipts, returns, and voids.
-   Cashier shifts and reconciliation.
-   Inventory integration, stock movement history, and conflict review.
-   Suppliers, purchase receiving, and stock adjustments, using existing
    capabilities where available.
-   Sales and stock reports.
-   Durable offline synchronization.
-   SaaS plan and subscription enforcement.

### 2.2 Business profiles

Use one POS platform with configurable modules rather than four separate
products.

**Retail** - Barcode scanning, product variants, discounts, returns,
cash payments, receipts.

**Grocery** - Fractional quantities and configurable units of measure. -
Weighted products and scale support only after hardware requirements are
validated. - Fast checkout and low-stock alerts.

**Wholesale** - Customer-specific price lists and quantity breaks. -
Bulk quantities and customer account/credit sales. - Customer balance
tracking if supported by the agreed accounting rules.

**Restaurant** - Tables, open orders, order modifications, kitchen
tickets, and split payments. - Keep the first restaurant workflow
narrow. Advanced kitchen routing, integrations, and restaurant
accounting can follow the initial pilot.

### 2.3 Explicitly defer

Unless required by pilot customers, defer: - Full accounting, payroll,
and tax filing. - Complex loyalty programs and promotions. - Advanced
restaurant kitchen display integrations. - Automated online card
processing. - Multi-currency settlement and foreign-exchange
management. - Microservices and complex distributed infrastructure.

## 3. Architecture

### 3.1 High-level design

``` mermaid
flowchart TD
    A[Flutter POS: Android and iOS] -->|HTTPS / Sync| B[ASP.NET Core API]
    C[Web POS and Admin Portal] -->|HTTPS| B
    A --- D[Local SQLite + Durable Outbox]
    C --- E[IndexedDB / Service Worker where supported]
    B --> F[Application Modules]
    F --> G[Existing Inventory System / Adapter]
    F --> H[Sales and Payments]
    F --> I[Synchronization]
    F --> J[Reporting]
    G --> K[(PostgreSQL)]
    H --> K
    I --> K
    J --> K
    B --> L[Background Worker]
    L --> M[Notifications / Exports / Retryable Jobs]
```

### 3.2 Backend architecture

Start with a **modular monolith**, not microservices. Keep modules
logically separated while deploying the API and worker as a small number
of services.

Suggested modules: - `Identity` - `Tenancy` - `Catalog` - `Pricing` -
`Sales` - `Payments` - `InventoryIntegration` - `Purchasing` -
`Customers` - `Restaurant` - `Synchronization` - `Reporting` -
`Subscriptions` - `PlatformAdmin`

Use application services for business workflows, explicit module
boundaries, and integration interfaces. Do not add generic repository
abstractions to every entity without a clear benefit.

### 3.3 Technology stack

  -----------------------------------------------------------------------
  Layer                               Recommendation
  ----------------------------------- -----------------------------------
  Mobile                              Flutter / Dart

  Mobile local storage                SQLite with migrations and a
                                      durable outbox

  Web POS and admin                   React + TypeScript, or Flutter Web
                                      if one UI codebase is more
                                      important than browser-specific
                                      ergonomics

  API                                 ASP.NET Core Web API

  Central database                    PostgreSQL

  Data access                         EF Core with Npgsql; Dapper/Npgsql
                                      for selected reporting queries

  Authentication                      ASP.NET Core Identity or an
                                      established identity provider

  Background work                     Hosted worker initially; add a
                                      durable queue when required

  Deployment                          Docker on Linux, HTTPS reverse
                                      proxy, backups, monitoring

  File storage                        Object storage for exports and
                                      product images, if needed
  -----------------------------------------------------------------------

**Framework lifecycle note:** .NET 8 support ends on November 10, 2026.
For a new commercial product being started in October 2026, prefer .NET
10 LTS unless a dependency or project constraint requires .NET 8.
Confirm current lifecycle and dependency compatibility before locking
the target framework.

### 3.4 Deployment approach

For the first pilot, the API, worker, and PostgreSQL can run on one
appropriately configured Linux host, provided backups are stored
separately. Keep the worker logically separate and design the deployment
so it can be moved to another host later.

Production basics: - TLS/HTTPS. - Automated PostgreSQL backups and
tested restores. - Structured logs and exception reporting. - Health
checks and basic service metrics. - Secrets outside source control. -
Database migrations run through a controlled deployment step. - Separate
development, staging, and production environments.

## 4. Existing Inventory System: Integration-First Plan

The inventory system already exists in the project. **Do not rebuild it
as part of the POS MVP.**

### 4.1 Discovery tasks before implementation

Inspect and document: 1. Existing product, category, barcode, variant,
and unit-of-measure models. 2. Existing branch/warehouse model and how
stock is partitioned. 3. Existing stock balance and stock
movement/transaction logic. 4. Existing purchase receiving, stock
adjustments, transfers, and returns. 5. Existing product pricing and
cost model. 6. Existing APIs, service classes, authorization rules, and
database migrations. 7. Whether multi-tenancy is already implemented. 8.
Whether the current database is PostgreSQL or another engine and what
migration strategy is needed. 9. Existing audit trail and concurrency
controls. 10. Existing tests and business rules that must not regress.

### 4.2 Integration rules

-   Keep one authoritative product catalog.
-   Keep one authoritative inventory ledger.
-   POS finalization must call the existing inventory application
    service or a well-defined adapter to record stock movements.
-   Avoid direct writes to inventory tables from POS code if the
    existing system owns those rules.
-   Reuse existing IDs where practical; otherwise, implement a
    documented mapping table.
-   Use database transactions where sale and inventory data share the
    same PostgreSQL database.
-   If the existing inventory system uses a different database or
    service, use an explicit integration contract and reliable
    messaging/outbox pattern rather than pretending a distributed
    transaction is atomic.
-   Add only missing POS-specific fields and tables.
-   Preserve existing API contracts unless a versioned change is agreed.
-   Add regression tests for inventory behaviors before integrating
    checkout.

### 4.3 Integration deliverable

Produce an **Inventory Integration Map** containing: - Existing
entity/table. - Existing service/API. - POS feature that consumes it. -
Changes required, if any. - Ownership of validation and writes. - Tests
needed. - Migration or compatibility risks.

Do not finalize the database schema until this discovery is complete.

## 5. Multi-Tenancy and Security

A tenant represents one subscribing business. A branch represents a shop
or location within that business.

Requirements: - Every tenant-owned record must be scoped to a tenant. -
The server derives tenant context from verified authentication and
membership, not from an untrusted tenant ID in a request body. - Every
query and command must verify tenant and branch access. - Use composite
foreign keys or equivalent constraints where practical to prevent
cross-tenant references. - PostgreSQL row-level security may provide
defense in depth, but does not replace application authorization. - If
using row-level security, set tenant context safely inside each
transaction and account for connection pooling. - Store secrets
securely; use platform-secure token storage on mobile. - Rotate refresh
tokens and support device/session revocation. - Audit sensitive actions
such as price overrides, refunds, voids, stock adjustments, and conflict
resolutions. - Enforce permission checks on the server even when the UI
hides restricted actions.

Suggested initial roles: - `Owner` - `Manager` - `Cashier` -
`InventoryClerk` - `RestaurantStaff` - `PlatformAdmin` (platform
operations, not a tenant role)

## 6. Offline-First Design

### 6.1 Local storage

Each offline-capable Flutter device should locally store: - Product and
barcode cache. - Applicable price lists and discount configuration. -
Minimum customer data required for checkout. - Branch and device
configuration. - Cached stock balances marked as potentially stale. -
Local sales and sale lines. - Local payment/tender records. - Durable
outbox commands. - Sync cursor and per-device sequence.

Use SQLite migrations and transaction boundaries. A sale, its lines, its
payment records, and its outbox command must be committed together
locally.

### 6.2 Checkout behavior

1.  Cashier scans or searches for products.
2.  App resolves product and price from the local cache.
3.  App calculates the cart using decimal-safe arithmetic and configured
    rounding rules.
4.  Cashier selects the payment method.
5.  App commits the finalized sale, lines, payments, and outbox command
    in one SQLite transaction.
6.  UI confirms that the sale is saved locally, and clearly indicates
    whether it is synchronized.
7.  A background sync process uploads commands when connectivity
    returns.

Do not require a network response before marking an offline sale as
locally saved.

### 6.3 Idempotent synchronization

Every command must have a globally unique UUID generated by the client.
The server must store the command ID and processing outcome in the same
PostgreSQL transaction as the business operation.

On retry: - If the command was committed, return the original result. -
If it is still processing, return a safe processing state. - If it
failed validation, return a stable error result. - Do not create a
second sale.

Use bounded sync batches and per-command results so one invalid command
does not erase unrelated valid commands.

### 6.4 Inventory conflicts

When devices sell stock offline, two devices can both sell the last
unit. A system disconnected from a shared authority cannot guarantee
globally correct stock availability.

MVP policy: - Permit offline checkout according to tenant
configuration. - Display when stock information is stale. - Consider
per-device offline stock allowances for high-risk products. -
Synchronize commands in order per device. - Never silently discard a
completed sale. - Flag negative balances or competing stock changes for
manager review. - Record conflict resolution in the audit log.

### 6.5 Web offline limitations

Browser storage such as IndexedDB can be cleared or evicted and has
different reliability characteristics from native SQLite. Make Flutter
the primary full-offline checkout client.

Web checkout should be online-first with best-effort offline support
unless testing proves the required guarantees. If reliable offline PC
checkout is mandatory, evaluate a desktop client or a local shop server.

## 7. Database Schema

The following is a **logical schema**, not a mandate to duplicate tables
already present in the inventory system. Map each proposed entity to the
existing implementation before creating migrations.

### 7.1 Tenant and identity

  -----------------------------------------------------------------------
  Table                               Key fields
  ----------------------------------- -----------------------------------
  `tenants`                           `id`, `name`, `status`,
                                      `created_at`

  `subscriptions`                     `id`, `tenant_id`, `plan_id`,
                                      `status`, `expires_at`

  `branches`                          `id`, `tenant_id`, `name`,
                                      `address`

  `users`                             `id`, `email`, `phone`,
                                      `password_hash`, `status`

  `tenant_users`                      `tenant_id`, `user_id`, `role_id`

  `roles` / `permissions`             IDs, names, permission keys

  `devices`                           `id`, `tenant_id`, `branch_id`,
                                      `device_code`, `last_sync_at`
  -----------------------------------------------------------------------

### 7.2 Catalog and pricing

First reuse the existing inventory catalog. Only add missing
capabilities.

  -----------------------------------------------------------------------
  Table                               Key fields
  ----------------------------------- -----------------------------------
  `categories`                        `id`, `tenant_id`, `parent_id`,
                                      `name`

  `products`                          `id`, `tenant_id`, `sku`, `name`,
                                      `category_id`, `track_stock`

  `product_barcodes`                  `id`, `tenant_id`, `product_id`,
                                      `barcode`, `unit_id`

  `product_variants`                  `id`, `product_id`, `attributes`

  `units_of_measure`                  `id`, `tenant_id`, `code`,
                                      `precision`

  `price_lists`                       `id`, `tenant_id`, `name`,
                                      `currency_code`

  `product_prices`                    `price_list_id`, `product_id`,
                                      `unit_price`

  `customers`                         `id`, `tenant_id`, `name`, `phone`

  `suppliers`                         `id`, `tenant_id`, `name`, `phone`
  -----------------------------------------------------------------------

### 7.3 Sales and payments

  -----------------------------------------------------------------------
  Table                               Key fields
  ----------------------------------- -----------------------------------
  `sales`                             `id`, `tenant_id`, `branch_id`,
                                      `device_id`, `receipt_no`,
                                      `status`, `sold_at`,
                                      `currency_code`, `subtotal`,
                                      `discount_total`, `tax_total`,
                                      `grand_total`

  `sale_lines`                        `id`, `sale_id`, `product_id`,
                                      `sku_snapshot`, `name_snapshot`,
                                      `quantity`, `unit_price`,
                                      `discount_amount`, `tax_amount`,
                                      `line_total`

  `payments`                          `id`, `sale_id`, `method`,
                                      `amount`, `status`, `reference`

  `refunds`                           `id`, `sale_id`, `status`,
                                      `amount`, `created_at`

  `refund_lines`                      `id`, `refund_id`, `sale_line_id`,
                                      `quantity`, `amount`

  `cash_shifts`                       `id`, `branch_id`, `device_id`,
                                      `cashier_id`, `opened_at`,
                                      `closed_at`

  `cash_movements`                    `id`, `cash_shift_id`, `type`,
                                      `amount`, `reason`
  -----------------------------------------------------------------------

Keep receipt-critical product and pricing values as sale-line snapshots
so catalog edits do not alter historical receipts.

### 7.4 Inventory integration

Reuse existing stock ledger and balance tables wherever possible. If
missing, the logical model is:

  -----------------------------------------------------------------------
  Table                               Key fields
  ----------------------------------- -----------------------------------
  `stock_movements`                   `id`, `tenant_id`, `branch_id`,
                                      `product_id`, `movement_type`,
                                      `quantity_delta`, `source_type`,
                                      `source_id`, `created_at`

  `stock_balances`                    `tenant_id`, `branch_id`,
                                      `product_id`, `quantity_on_hand`,
                                      `updated_at`

  `inventory_conflicts`               `id`, `tenant_id`, `branch_id`,
                                      `product_id`, `source_command_id`,
                                      `status`
  -----------------------------------------------------------------------

The inventory ledger should be the authoritative history; balances
should be a consistent projection or maintained aggregate.

### 7.5 Purchasing and restaurant

Purchasing, if not already implemented: - `purchase_orders` -
`goods_receipts` - `purchase_order_lines` - `goods_receipt_lines`

Restaurant: - `restaurant_tables` - `restaurant_orders` -
`restaurant_order_lines` - `product_modifiers` - `modifier_options` -
`kitchen_tickets`

### 7.6 Synchronization and audit

  -----------------------------------------------------------------------
  Table                               Key fields
  ----------------------------------- -----------------------------------
  `sync_commands`                     `id`, `tenant_id`, `device_id`,
                                      `command_type`, `payload_hash`,
                                      `status`, `result`, `created_at`

  `sync_cursors`                      `tenant_id`, `device_id`,
                                      `last_server_sequence`

  `device_sequences`                  `device_id`,
                                      `last_accepted_sequence`

  `outbox_events`                     `id`, `event_type`, `aggregate_id`,
                                      `payload`, `created_at`,
                                      `processed_at`

  `audit_logs`                        `id`, `tenant_id`, `actor_id`,
                                      `action`, `entity_type`,
                                      `entity_id`, `created_at`
  -----------------------------------------------------------------------

The client outbox is stored in SQLite. The server outbox is for reliable
post-commit work. They serve different purposes.

### 7.7 PostgreSQL data rules

-   Use UUID primary keys for client/server-shared entities.
-   Use `NUMERIC`, not floating point, for money and decimal quantities.
-   Store timestamps consistently, preferably as UTC `timestamptz`.
-   Use tenant-scoped unique constraints for SKUs and barcodes where
    business rules permit.
-   Enforce unique receipt numbers within the appropriate tenant/branch
    scope.
-   Use foreign keys, check constraints, and useful indexes.
-   Store currency codes explicitly. Default to LYD but avoid
    hard-coding currency into calculation logic.
-   Decide quantity precision and monetary rounding rules before
    implementation.
-   Use migrations and test them against realistic data volumes.

## 8. API Endpoint Plan

Prefix endpoints with `/api/v1`. All non-public endpoints require
authentication and authorization.

### Authentication and tenant setup

  Method   Endpoint              Purpose
  -------- --------------------- --------------------------------
  POST     `/auth/login`         Sign in
  POST     `/auth/refresh`       Rotate refresh token
  POST     `/auth/logout`        Revoke session
  GET      `/me`                 Current user and permissions
  GET      `/tenants/current`    Current business configuration
  GET      `/branches`           Accessible branches
  POST     `/devices/register`   Register POS device

### Products and pricing

  Method   Endpoint                           Purpose
  -------- ---------------------------------- ---------------------------------------------
  GET      `/products`                        Search and paginate products
  GET      `/products/{id}`                   Product details
  GET      `/products/by-barcode/{barcode}`   Resolve barcode
  POST     `/products`                        Create product if catalog ownership permits
  PUT      `/products/{id}`                   Update product if catalog ownership permits
  GET      `/prices`                          Download applicable prices
  POST     `/products/import`                 CSV import, if not already supported

If the inventory system already owns product management, POS should call
its existing service/API instead of creating competing product
endpoints.

### Sales and payments

  Method   Endpoint                    Purpose
  -------- --------------------------- ---------------------------
  POST     `/sales`                    Finalize an online sale
  GET      `/sales/{id}`               Sale details
  GET      `/sales`                    Search sales
  POST     `/sales/{id}/returns`       Process return
  POST     `/sales/{id}/void`          Void eligible sale
  GET      `/sales/{id}/receipt`       Receipt data
  POST     `/cash-shifts/open`         Open cashier shift
  POST     `/cash-shifts/{id}/close`   Close and reconcile shift

### Inventory and purchasing

  Method   Endpoint                   Purpose
  -------- -------------------------- -------------------------
  GET      `/inventory/balances`      Stock by branch/product
  GET      `/inventory/movements`     Stock movement history
  POST     `/inventory/adjustments`   Authorized adjustment
  POST     `/inventory/transfers`     Transfer stock
  POST     `/purchase-orders`         Create purchase order
  POST     `/goods-receipts`          Receive stock
  GET      `/suppliers`               Supplier management

Map these to the existing inventory API or application services wherever
available.

### Offline synchronization

  --------------------------------------------------------------------------------
  Method                  Endpoint                         Purpose
  ----------------------- -------------------------------- -----------------------
  GET                     `/sync/bootstrap`                Initial device snapshot

  POST                    `/sync/push`                     Upload bounded command
                                                           batch

  GET                     `/sync/pull?cursor={cursor}`     Download incremental
                                                           changes

  GET                     `/sync/commands/{id}`            Check command outcome

  GET                     `/sync/conflicts`                List conflicts for
                                                           authorized users

  POST                    `/sync/conflicts/{id}/resolve`   Resolve conflict with
                                                           audit record
  --------------------------------------------------------------------------------

Use a stable per-command result contract. One invalid command should not
discard unrelated valid commands in a batch. The server must generate
and validate cursor values and must not trust client-supplied prices or
totals without applying business rules.

### Reporting and restaurant

  Method   Endpoint                             Purpose
  -------- ------------------------------------ ---------------------------
  GET      `/reports/sales-summary`             Daily/monthly sales
  GET      `/reports/gross-profit`              Margin report
  GET      `/reports/stock`                     Stock report
  GET      `/reports/cashier-shifts`            Cash reconciliation
  GET      `/restaurant/tables`                 Table layout/status
  POST     `/restaurant/orders`                 Open order
  PATCH    `/restaurant/orders/{id}`            Modify open order
  POST     `/restaurant/orders/{id}/submit`     Submit preparation ticket
  POST     `/restaurant/orders/{id}/checkout`   Finalize order/payment

## 9. MVP Backlog

Priority definitions: **P0** required for pilot; **P1** launch
enhancement; **P2** later release.

  -----------------------------------------------------------------------
  Feature                 Priority                Acceptance criteria
  ----------------------- ----------------------- -----------------------
  Inventory system        P0                      Existing catalog and
  discovery and adapter                           stock rules are
                                                  documented and reused

  Tenant isolation        P0                      Cross-tenant access
                                                  tests pass

  Users and permissions   P0                      Server enforces
                                                  cashier/manager/owner
                                                  permissions

  Barcode and product     P0                      Existing catalog
  lookup                                          resolves products
                                                  quickly

  Mobile checkout         P0                      Sale saves locally
                                                  without internet

  Cash payment and        P0                      Totals reconcile and
  receipt                                         receipts can be
                                                  reprinted

  Durable local outbox    P0                      Pending sales survive
                                                  app restart

  Idempotent sync         P0                      Replaying a command
                                                  never duplicates a sale

  Inventory ledger        P0                      Sales create consistent
  integration                                     stock movements

  Conflict review         P0                      Conflicts are visible
                                                  and auditable

  Branch/device           P0                      Device is bound to
  registration                                    authorized
                                                  tenant/branch

  Web admin and online    P0                      Owners manage shop and
  checkout                                        online cashiers can
                                                  sell

  Cash shifts and basic   P0                      Cash and sales totals
  reports                                         can be reconciled

  CSV import              P1                      Shop can import an
                                                  existing catalog

  Purchasing              P1                      Receiving stock updates
                                                  inventory correctly

  Wholesale price         P1                      Customer pricing and
  lists/credit                                    balances are tracked

  Grocery fractional      P1                      Configured precision is
  quantities                                      respected

  Restaurant tables/open  P1                      Orders can be created,
  orders                                          changed and checked out

  Printer compatibility   P1                      Supported devices print
                                                  readable Arabic/English
                                                  receipts

  Subscription            P1                      Plan limits and
  enforcement                                     expiration are enforced

  Loyalty and advanced    P2                      Added after core
  promotions                                      reliability

  Full accounting/payroll P2                      Separate scope or
                                                  integration
  -----------------------------------------------------------------------

## 10. Development Sequence

Indicative timeline: **14--20 weeks** for a tightly scoped pilot,
depending on team size, existing inventory architecture, printer
support, and the depth of restaurant/wholesale workflows.

### Phase 0 --- Existing system discovery (1 week)

-   Map inventory entities, APIs, services, and stock rules.
-   Identify existing authentication and tenant model.
-   Review database engine and migrations.
-   Add or verify regression tests.
-   Produce the Inventory Integration Map.
-   Decide whether POS and inventory share one PostgreSQL database or
    require a service boundary.

**Exit criteria:** no duplicate product catalog or stock ledger is
planned without an explicit reason.

### Phase 1 --- Foundation (weeks 1--2)

-   Set up source control, CI, Docker, and environments.
-   Configure PostgreSQL and migrations.
-   Implement/verify tenant isolation, authentication, roles, branches
    and devices.
-   Establish structured logging, error handling and health checks.

**Exit criteria:** secure, deployable API skeleton and automated tenant
isolation tests.

### Phase 2 --- Catalog and online checkout (weeks 3--5)

-   Integrate existing product/barcode search and price logic.
-   Build cart and sale-line snapshots.
-   Implement online sale finalization and cash payments.
-   Integrate inventory movement in a transaction where database
    boundaries permit.
-   Add receipt generation and basic sales queries.

**Exit criteria:** online checkout is reliable and inventory updates
exactly once.

### Phase 3 --- Offline engine (weeks 6--9)

-   Design SQLite schema and migrations.
-   Implement local sale transaction and durable outbox.
-   Implement sync push/pull, idempotency and per-command results.
-   Add cursor handling, retry behavior and conflict review.
-   Test crashes, timeouts, duplicate submissions and concurrent sales.

**Exit criteria:** a sale survives offline use and restarts, and can
synchronize safely later.

### Phase 4 --- Web and hardware (weeks 10--12)

-   Build web administration and online web checkout.
-   Package Flutter apps for Android and iOS.
-   Implement Arabic RTL and English localization.
-   Test camera scanning, Bluetooth scanners, printers and receipt
    rendering.
-   Document the supported hardware list.

**Exit criteria:** target device matrix passes the checkout and printing
tests.

### Phase 5 --- Retail operations and business profiles (weeks 13--16)

-   Add cash shifts, returns and stock receiving as needed.
-   Build basic reports.
-   Add minimum wholesale pricing/credit workflows.
-   Add minimum restaurant tables/open orders workflow.
-   Add grocery units/fractional quantities where required.

**Exit criteria:** agreed pilot profiles can complete their primary
workflows without bypassing business rules.

### Phase 6 --- Pilot and hardening (weeks 17--20)

-   Pilot with 3--5 shops.
-   Test restore procedures, device loss, network outages and sync
    backlog.
-   Review tenant security, permissions and audit trails.
-   Measure sync failures, checkout duration and support issues.
-   Fix high-frequency problems before general availability.

**Exit criteria:** documented limitations, tested recovery procedures,
and no known critical data-integrity defects.

## 11. Repository Structure

``` text
pos-platform/
├── src/
│   ├── Pos.Api/
│   ├── Pos.Worker/
│   ├── Modules/
│   │   ├── Identity/
│   │   ├── Tenancy/
│   │   ├── Catalog/
│   │   ├── Pricing/
│   │   ├── Sales/
│   │   ├── Payments/
│   │   ├── InventoryIntegration/
│   │   ├── Purchasing/
│   │   ├── Restaurant/
│   │   ├── Synchronization/
│   │   └── Reporting/
│   └── Shared/
├── clients/
│   ├── pos_mobile/
│   └── pos_web/
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   ├── TenantIsolationTests/
│   ├── InventoryIntegrationTests/
│   └── SyncTests/
├── deploy/
│   ├── docker/
│   └── scripts/
└── docs/
    ├── architecture/
    ├── api/
    ├── inventory-integration/
    └── operations/
```

This is a logical layout. Do not create a separate deployable service
for each module unless future scale or team boundaries justify it.

## 12. Testing Strategy

### Unit tests

-   Price calculations, discounts, tax/rounding policies.
-   Quantity precision and unit conversions.
-   Permission policies and business rules.
-   Idempotency behavior and command-state transitions.

### Integration tests

-   Sale + payment + inventory movement atomicity.
-   PostgreSQL constraints and transaction rollback.
-   Existing inventory adapter behavior.
-   Refund and void workflows.
-   Tenant and branch access enforcement.

### Synchronization tests

-   Same command sent multiple times.
-   Request times out after server commit.
-   Device restarts with pending commands.
-   Commands arrive out of order.
-   One command fails within a batch.
-   Two offline devices sell the same final unit.
-   Cursor expires or a device requires a fresh bootstrap.
-   Device is revoked while it has unsynchronized sales.

### End-to-end tests

-   Scan product → pay cash → print receipt.
-   Complete sale offline → restart app → reconnect → sync.
-   Return an item with correct inventory effect.
-   Cashier attempts manager-only action.
-   Tenant A attempts to access Tenant B data.
-   Arabic and English receipt tests on supported printers.

## 13. Production Readiness Checklist

-   [ ] Inventory integration map approved.
-   [ ] No duplicate product catalog or stock ledger.
-   [ ] Tenant isolation tests pass.
-   [ ] Sales, payments, and stock movements have correct transaction
    boundaries.
-   [ ] Local sales and outbox commit atomically.
-   [ ] Sync commands are idempotent and replay-safe.
-   [ ] Conflicts never silently discard completed sales.
-   [ ] Device revocation and token rotation are supported.
-   [ ] Money uses decimal/numeric arithmetic with defined rounding.
-   [ ] Receipt snapshots preserve historical product and price details.
-   [ ] PostgreSQL backups and restore drills are tested.
-   [ ] Logs and metrics expose synchronization failures.
-   [ ] Supported scanner/printer models are documented.
-   [ ] Arabic RTL and printed Arabic are tested.
-   [ ] Libyan invoicing, tax, and accounting obligations are reviewed
    with a qualified local adviser.
-   [ ] Subscription plans, support processes, and tenant provisioning
    are documented.

## 14. Recommended Immediate Next Steps

1.  Inspect the existing inventory project and create the Inventory
    Integration Map.
2.  Confirm the current database engine, authentication, tenant model,
    and branch/warehouse model.
3.  Agree on the authoritative sale-finalization and stock-movement
    transaction boundary.
4.  Define the local SQLite schema and sync command contract.
5.  Implement one vertical slice: cached product lookup → offline cash
    sale → durable outbox → server sync → inventory update → receipt.
6.  Test the vertical slice under outages and duplicate requests before
    expanding features.
7.  Add web checkout, reports, wholesale, grocery, and restaurant
    workflows incrementally.

## Final Principle

**Extend the existing inventory system; do not replace it.** The POS
should be a new checkout and synchronization capability that uses the
project's established product and stock rules. Build the offline
transaction pipeline and inventory integration first, because those
decisions determine the reliability of every other feature.
