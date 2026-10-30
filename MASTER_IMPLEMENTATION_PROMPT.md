# WAREHOUSE SaaS — MASTER IMPLEMENTATION PROMPT

## Cursor Agent Instructions — Continue From Existing Database Foundation

You are continuing an existing Warehouse Management SaaS project.

IMPORTANT:
- Do NOT restart or recreate the project.
- Do NOT replace the existing architecture.
- Inspect the repository before changing anything.
- Treat the existing `docs/` files, architecture tests, database foundation, and current code as the source of truth.
- Preserve all working code and tests unless a change is required.
- Work incrementally.
- After every phase, build and test the solution.
- Never silently change an architectural or business decision.
- Do not implement future phases early.
- Do not generate fake/demo business logic just to make an endpoint compile.

---

# 1. CURRENT PROJECT STATE

The repository already contains:

- .NET 8 solution
- `Warehouse.Domain`
- `Warehouse.Application`
- `Warehouse.Infrastructure`
- `Warehouse.Shared`
- `Warehouse.Api`
- Architecture tests
- Unit tests
- Integration-test infrastructure
- PostgreSQL + EF Core foundation
- Multi-tenant foundation
- Tenant-owned entities
- Initial migration
- System permissions seed
- Offline/synchronization foundation
- Documentation under `docs/`

The database foundation has already been implemented.

Existing foundation includes concepts such as:

- Tenant
- User
- Role
- Permission
- RolePermission
- UserRole
- Warehouse
- WarehouseLocation
- Device
- AuditLog
- ProcessedSyncOperation
- DocumentLifecycleStatus
- NegativeStockPolicy
- related enums/helpers

The current database design uses:

- PostgreSQL
- EF Core
- snake_case database naming
- UUID primary keys
- `tenant_id` on tenant-owned tables
- tenant query filters
- tenant-aware uniqueness constraints
- audit infrastructure
- device/synchronization infrastructure
- initial migration
- system permission seed

Do not undo this foundation.

---

# 2. ARCHITECTURE

Maintain dependency direction:

    Warehouse.Domain
          ↓
    Warehouse.Application
          ↓
    Warehouse.Infrastructure
          ↓
    Warehouse.Api

`Warehouse.Shared` may contain genuinely shared primitives/contracts only.

Do not introduce circular dependencies.

Do not place business rules in controllers.

Do not put EF Core implementation details in Domain entities.

Do not bypass Application from API controllers.

Use:
- Domain for entities, value objects and domain rules.
- Application for commands, queries, DTOs, validation and interfaces.
- Infrastructure for EF Core, PostgreSQL, authentication and sync persistence.
- API for HTTP transport, authentication, authorization and endpoint composition.

Architecture tests must continue enforcing these rules.

---

# 3. PRODUCT GOAL

Build a production-grade Warehouse Management SaaS supporting:

1. Multi-tenant SaaS
2. Multiple warehouses
3. Warehouse locations
4. Products/catalog
5. Product variants/SKUs
6. Units of measure
7. Inventory
8. Stock movements
9. Stock adjustments
10. Stock transfers
11. Purchasing
12. Sales
13. Customers
14. Suppliers
15. Payment status tracking
16. Users, roles and permissions
17. Audit logging
18. Offline-first desktop/web operation
19. Offline-first mobile operation
20. Synchronization
21. Conflict detection/resolution
22. Device registration
23. Tenant settings
24. Tax configuration
25. Currency configuration
26. Reporting foundations
27. SaaS readiness

The system must work both online as SaaS and offline on a customer device.

---

# 4. NON-NEGOTIABLE DATABASE RULES

Preserve:

- UUID primary keys.
- Client-generated IDs for offline-created entities.
- `tenant_id NOT NULL` for tenant-owned entities.
- Tenant query isolation.
- No cross-tenant data access.
- Tenant-aware unique indexes.
- UTC timestamps.
- Money: `numeric(19,4)`.
- Quantity: `numeric(18,6)`.
- PostgreSQL.
- EF Core.
- snake_case naming.

Money and quantity must never use floating point.

Use .NET `decimal`.

---

# 5. TENANCY

Every tenant-owned record belongs to exactly one tenant.

Tenant isolation must exist through:

1. Application tenant context.
2. EF Core query filters where appropriate.
3. Database constraints/indexes.
4. Authorization.
5. Integration tests proving cross-tenant isolation.

Never trust client-supplied `tenant_id`.

The authenticated user's tenant determines the tenant.

---

# 6. TRANSFERS / IN-TRANSIT

Use a dedicated `IN_TRANSIT` warehouse location according to the existing architecture.

When dispatched:
- stock leaves source
- stock enters in-transit

When received:
- stock leaves in-transit
- stock enters destination

A transfer must never make stock disappear.

Every stock movement must be auditable.

Transfer operations must be idempotent.

Duplicate sync requests must not duplicate stock movements.

---

# 7. INVENTORY

Implement:

- Product
- SKU/product code
- Warehouse
- Location
- Quantity on hand
- Reserved quantity
- Available quantity
- Stock movement history
- Adjustment
- Transfer
- Opening balance

Use:

    Available = OnHand - Reserved

Every inventory-changing operation creates an immutable stock movement.

Examples:
- Purchase receipt
- Sales shipment
- Sales return
- Purchase return
- Adjustment
- Transfer dispatch
- Transfer receive
- Opening balance

Never directly change inventory without the corresponding movement.

---

# 8. NEGATIVE STOCK

Respect the existing tenant-level `NegativeStockPolicy`.

Support the policies already defined in the code/docs, such as:
- Allow
- Prevent
- Warn

Do not hard-code one policy.

---

# 9. PRODUCT CATALOG

Implement:

- Product
- Category where required
- SKU/code
- Barcode
- Name
- Description
- Brand if required
- Unit
- Purchase price
- Sale price
- Tax configuration
- Active/inactive
- Track inventory
- Minimum stock
- Maximum stock
- Reorder point

Uniqueness is tenant-scoped.

Example:

    (tenant_id, code)

Barcode uniqueness should also be tenant-scoped when required.

---

# 10. UNITS OF MEASURE

Support units such as:

- piece
- box
- carton
- kg
- gram
- liter
- meter

Implement conversion only when required by the existing specification.

Quantities use `numeric(18,6)`.

---

# 11. WAREHOUSES AND LOCATIONS

A tenant can have multiple warehouses.

A warehouse can have multiple locations.

Respect existing location kinds including:
- GENERAL
- IN_TRANSIT

Tenant-scoped uniqueness:

    (tenant_id, warehouse_id, code)

Never allow cross-tenant warehouse/location relationships.

---

# 12. PURCHASING

Implement concepts such as:

- Supplier
- PurchaseOrder
- PurchaseOrderLine
- PurchaseReceipt
- PurchaseReceiptLine
- PurchaseReturn
- PurchaseReturnLine

Lifecycle:

    Draft
      ↓
    Confirmed
      ↓
    Partially Received
      ↓
    Fully Received
      ↓
    Closed

Purchase order confirmation alone must not increase physical stock.

Receiving must create stock movements.

Support partial receiving and duplicate-receipt protection.

---

# 13. SALES

Implement:

- Customer
- SalesOrder
- SalesOrderLine
- Shipment
- ShipmentLine
- SalesReturn
- SalesReturnLine
- Payment/status tracking

Lifecycle:

    Draft
      ↓
    Confirmed
      ↓
    Partially Fulfilled
      ↓
    Fulfilled
      ↓
    Closed

Creating an order must not automatically decrease physical stock.

Stock changes at the defined fulfillment/shipment event.

Support partial fulfillment and returns.

---

# 14. PAYMENT STATUS

The first version does NOT require a general ledger.

Commercial documents support:

- Unpaid
- PartiallyPaid
- Paid

Do not implement accounting/general-ledger functionality unless explicitly requested later.

---

# 15. TAX

Tax is disabled by default for a tenant.

If disabled:
- tax rate defaults to zero
- tax calculations remain inactive

If enabled:
- configured rates may be applied

Tax rates must never be negative.

Offline clients must receive enough configuration to calculate documents offline.

---

# 16. CURRENCY

Currency belongs to tenant configuration.

Money uses:

    numeric(19,4)

Do not use `double`.

Do not silently implement currency conversion.

---

# 17. AUTHENTICATION

Implement:

- Login
- Access token
- Refresh token
- Logout/revoke
- Current user
- Device registration
- Tenant membership
- Roles
- Permissions

Use established ASP.NET Core security mechanisms.

Never store plaintext passwords.

Use secure refresh-token handling.

---

# 18. AUTHORIZATION

Use permissions rather than hard-coded role names.

Examples:

- product.read
- product.create
- product.update
- product.delete
- inventory.read
- inventory.adjust
- inventory.transfer
- purchasing.read
- purchasing.create
- purchasing.receive
- sales.read
- sales.create
- sales.ship
- reports.read
- users.manage
- roles.manage
- tenant.manage

Authorization must be enforced server-side.

---

# 19. OFFLINE-FIRST

Offline is a core product requirement.

Respect existing:

- `Tenant.MaxOfflineDays`
- `RequireOnlineRevalidation`
- `Device.LastOnlineAuth`
- device status
- `User.IsDisabled`
- tenant suspension state

Offline work is allowed only within the configured security policy.

Do not disable security simply because the client is offline.

---

# 20. OFFLINE DATA

Offline-capable clients need local persistence.

Maintain:

- local entities
- pending operations
- operation IDs
- local timestamps
- sync state
- conflict state

Every offline-created entity uses a client-generated UUID.

---

# 21. SYNCHRONIZATION

Implement reliable synchronization.

## Push

Client sends:

- OperationId
- DeviceId
- TenantId
- EntityType
- EntityId
- OperationType
- Payload
- ClientTimestamp
- BaseVersion/concurrency information

Server:

1. Authenticates device/user.
2. Validates tenant.
3. Checks idempotency.
4. Applies operation.
5. Records processed operation.
6. Returns result.

Duplicate OperationId must never repeat the business operation.

## Pull

Implement incremental change feed when required by the architecture.

Return:

- changed entities
- deleted entities/tombstones
- next cursor/version

Never download the entire database repeatedly.

---

# 22. CONFLICT RESOLUTION

Never silently overwrite important business data.

Examples:

Master data:
- last-write-wins only when explicitly documented.

Inventory:
- never overwrite balance blindly
- resolve through stock movements

Sales/purchases:
- validate business state transitions

Transfers:
- validate state machine

Users/permissions:
- server authoritative

Use explicit outcomes:

- Applied
- AlreadyProcessed
- Conflict
- Rejected
- ValidationFailed

Maintain:

    docs/conflict-resolution.md

---

# 23. AUDIT LOG

Important business operations must be auditable.

Include as appropriate:

- Id
- TenantId
- UserId
- DeviceId
- Action
- EntityType
- EntityId
- Timestamp UTC
- CorrelationId
- Result/status
- Metadata

Never log passwords, tokens or secrets.

Document retention.

---

# 24. IDEMPOTENCY

Use the existing `ProcessedSyncOperation` foundation.

If the client sends operation ABC, the server processes it, the response is lost, and ABC is sent again:

- return the previous result/equivalent idempotent response
- do not create duplicate documents
- do not duplicate stock movements
- do not duplicate payments

---

# 25. API

Build REST APIs following the existing conventions.

At minimum:

- Authentication
- Products
- Warehouses
- Locations
- Inventory
- Purchases
- Sales
- Transfers
- Users
- Roles/permissions
- Sync
- Health

Keep:

    GET /health/live

Use DTOs, not EF entities.

Validate all input.

Use consistent errors and HTTP status codes.

---

# 26. ERROR CONTRACT

Include:

- error code
- message
- correlation ID
- validation details when applicable

Never expose stack traces or raw database exceptions in production.

---

# 27. CONCURRENCY

Use optimistic concurrency/versioning where appropriate for:

- Product
- Warehouse/location configuration
- Sales order state
- Purchase order state
- Transfer state
- Inventory operations

Do not rely only on client validation.

---

# 28. MOBILE

Create/continue Flutter client in:

    clients/mobile

Support:

- Login
- Local database
- Device registration
- Product lookup
- Barcode scanning
- Inventory
- Stock adjustment
- Transfers
- Receiving
- Sales/picking where applicable
- Offline queue
- Sync status
- Conflict display
- Re-authentication

Do not make every screen dependent on a live API.

---

# 29. WEB / DESKTOP

Use the existing frontend if present. If none exists, use React + TypeScript.

Support:

- Dashboard
- Products
- Warehouses
- Locations
- Inventory
- Purchases
- Sales
- Transfers
- Customers
- Suppliers
- Users
- Roles
- Reports
- Tenant settings
- Sync status

Offline capability must be designed from the beginning.

---

# 30. SAAS

Prepare for SaaS with:

- Tenant lifecycle
- Tenant status
- Currency
- Tax configuration
- Offline policy
- Subscription/readiness metadata

Keep billing provider integration abstract.

Do not integrate Stripe or another payment provider unless explicitly requested.

---

# 31. SECURITY

Mandatory:

- HTTPS in production
- Secure password hashing
- Short-lived access tokens
- Secure refresh-token handling
- Tenant isolation
- Permission checks
- Input validation
- Rate limiting where appropriate
- No secrets in source control
- Environment variables/secret store
- Secure CORS
- Security headers where appropriate
- Audit logging
- Device/session revocation

Never log secrets.

---

# 32. TESTING

Every module requires tests.

## Unit
Domain/business rules.

## Application
Commands, queries, validation.

## Integration
PostgreSQL + EF Core.

## Architecture
Dependency direction.

## Security
Tenant isolation and authorization.

## Synchronization
- first push
- duplicate push
- conflict
- offline create
- offline update
- pull
- retry
- device revocation

## Inventory
- receive
- sell
- return
- adjust
- transfer dispatch
- transfer receive
- negative stock
- duplicate operation

## Multi-tenant
Tenant A must never see Tenant B data.

---

# 33. DOCKER

Provide:

- Dockerfile
- docker-compose.yml
- PostgreSQL
- API where appropriate

If Docker is unavailable on the development machine, do not claim Docker tests passed. Report them as skipped.

---

# 34. DOCUMENTATION

Maintain:

    docs/architecture.md
    docs/database.md
    docs/offline-first.md
    docs/synchronization.md
    docs/conflict-resolution.md
    docs/business-rules.md
    docs/security.md
    docs/api.md
    docs/deployment.md
    README.md

Documentation must match actual implementation.

---

# 35. IMPLEMENTATION ORDER

Do NOT implement everything in one giant change.

## PHASE 1 — Remaining database decisions

Before business modules, inspect existing code/docs and resolve:

1. TransferLine → StockMovement FK shape.
2. Per-tenant system-role provisioning.
3. Refresh-token table shape.
4. Pull synchronization change-log table.
5. Exact in-transit stock representation.
6. Whether location_id is nullable in any balance table.
7. Audit-log columns and retention.
8. processed_sync_operations retention/partitioning.
9. Tax-disabled defaults.
10. EF Core snake_case configuration.
11. Remaining constraints/indexes.

For each:
- inspect code/docs
- choose a consistent design
- update docs
- add/update tests

STOP after Phase 1 and report the decisions.

---

## PHASE 2 — Authentication and authorization

Implement login, refresh, logout/revoke, current user, device registration, roles, permissions and tenant context.

Add tests.

STOP and report.

---

## PHASE 3 — Product catalog

Implement products, SKUs, barcodes, units, pricing and tax integration.

Add tests.

STOP and report.

---

## PHASE 4 — Inventory

Implement balances, stock movements, adjustments, opening balances and negative-stock policy.

Add tests.

STOP and report.

---

## PHASE 5 — Transfers

Implement transfer, lines, dispatch, in-transit, receive, cancellation, idempotency and concurrency.

Add tests.

STOP and report.

---

## PHASE 6 — Purchasing

Implement suppliers, purchase orders, receiving, returns and payment status.

Add tests.

STOP and report.

---

## PHASE 7 — Sales

Implement customers, sales orders, fulfillment, returns and payment status.

Add tests.

STOP and report.

---

## PHASE 8 — Synchronization

Implement push, pull, cursor/change feed, idempotency, conflict detection, conflict response, tombstones and retry behavior.

Add comprehensive tests.

STOP and report.

---

## PHASE 9 — Mobile

Implement Flutter local database, authentication, offline queue, synchronization, product lookup, inventory and barcode scanning, then business workflows.

STOP and report.

---

## PHASE 10 — Web client

Implement the web client using the existing frontend or React + TypeScript.

Prioritize authentication, dashboard, products, inventory, purchasing, sales, transfers, administration and synchronization status.

STOP and report.

---

## PHASE 11 — SaaS platform

Implement tenant administration, tenant lifecycle, subscription abstraction, usage/readiness metrics and platform administration.

Do not integrate a payment provider unless explicitly requested.

---

## PHASE 12 — Production hardening

Implement security hardening, rate limiting, observability, structured logging, health checks, metrics, backup documentation, Docker deployment, production configuration, CI/CD and migration strategy.

---

# 36. CURSOR EXECUTION RULES

For every task:

1. Read relevant `docs/*.md`.
2. Search the repository for existing related code.
3. Do not duplicate abstractions.
4. Make the smallest coherent change.
5. Add tests.
6. Run:

    dotnet build Warehouse.sln -c Release

7. Run:

    dotnet test Warehouse.sln -c Release

8. Run integration tests when Docker is available.
9. Report skipped tests honestly.
10. Update documentation.
11. Review the diff.
12. Check for breaking changes.
13. Only then continue.

---

# 37. STOP CONDITIONS

Ask for clarification if:

- a business rule is genuinely ambiguous
- architecture documents contradict each other
- a destructive database migration is required
- an existing API contract would be broken
- tenant isolation cannot be guaranteed
- offline synchronization semantics are unclear
- a decision would permanently constrain the product

Do not stop for trivial implementation choices already covered here.

---

# 38. FIRST ACTION

Start by inspecting:

- repository tree
- solution/projects
- docs/
- database entities
- DbContext
- migrations
- architecture tests
- current tests
- API
- offline/sync documentation

Then compare the repository against this prompt.

DO NOT implement business modules yet.

First produce:

1. Current state summary.
2. Conflicts found.
3. Remaining database decisions.
4. Proposed decisions.
5. Files that would change.
6. Tests that will prove correctness.

Then wait for confirmation.

---

# 39. FINAL RULE

This is a real SaaS product, not a demo.

Prefer:

- correctness over speed
- explicit business rules over magic
- transactional integrity over convenience
- idempotency over assumptions
- tenant isolation over shortcuts
- offline safety over simplistic APIs
- test coverage over "it compiles"
- maintainability over generated complexity

Never claim a feature is implemented unless it exists in the repository and has been tested.
