# Business rules

Status: Phase 1 decisions locked. No business module is implemented.

Module-specific rules will be added later as `docs/business-rules-<module>.md` (Windows cannot host both `business-rules.md` and a `business-rules/` directory).

Each module document must include: Valid states, Transitions, Permissions, Inventory effects, Offline behavior, Synchronization behavior, Error conditions.

## Locked cross-module rules

### Tenancy

- One shared PostgreSQL schema.
- Every tenant-owned row has non-null `TenantId` / `tenant_id`.
- Tenant scope from authenticated identity + EF query filters.
- Composite indexes lead with `tenant_id`.

### Money and quantity

- Money: `numeric(19,4)`. Quantity: `numeric(18,6)`. No float/double.
- Available stock quantity math uses decimals; Available itself is not stored (see StockBalance).

### StockBalance (Phase 4)

- Columns: Id, TenantId, ProductId, WarehouseId, LocationId (**NOT NULL**), OnHand, Reserved.
- **Available = OnHand - Reserved** (calculated only).
- `IN_TRANSIT` is a real warehouse location; in-transit stock is OnHand there. No separate in-transit balance type.

### Inventory ledger (Phase 4)

- StockMovement is authoritative and normally immutable; corrections are compensating movements.
- Stock changes on document **Posted**, not Draft.
- Negative stock policies: **Prevent**, **Allow**, **AllowWithPermission** only. Do not add Warn.
- Core inventory posting does not compute tax.

### Transfers (Phase 5)

- Flow: source location → source warehouse `IN_TRANSIT` → destination location.
- Dispatch / receive update OnHand; atomic and idempotent; partial receive supported.
- StockMovement uses nullable TransferLineId + SourceDocumentType/SourceDocumentId.

### Purchasing (Phase 6)

- Lifecycle: Draft → Confirmed → PartiallyReceived → FullyReceived → Closed (or Cancelled).
- Confirm does **not** change stock. Receive increases OnHand; return decreases OnHand.
- Payment status: Unpaid / PartiallyPaid / Paid. No GL.
- Partial receive and idempotent `operationId` supported.

### Sales (Phase 7)

- Lifecycle: Draft → Confirmed → PartiallyFulfilled → Fulfilled → Closed (or Cancelled).
- Confirm does **not** change stock. Ship decreases OnHand; return increases OnHand.
- Payment status: Unpaid / PartiallyPaid / Paid. No GL.
- Partial ship and idempotent `operationId` supported.

### Document lifecycle

- Generic: Draft, Approved, Posted, Cancelled (`DocumentLifecycleStatus`).
- Purchasing uses module-specific `PurchaseOrderStatus` without changing `DocumentLifecycleStatus`.
- Sales uses module-specific `SalesOrderStatus` without changing `DocumentLifecycleStatus`.

### Tax and payment

- Tax disabled by default on tenant.
- Commercial payment status: Unpaid, PartiallyPaid, Paid. No GL in v1.

### Offline and security

- Default **MaxOfflineDays = 7** (tenant-configurable; `OfflineSessionPolicy.DefaultMaxOfflineDays`). Do not hard-code the literal into handlers.
- After expiry: reconnect and revalidate before further offline business work.
- Also: RequireOnlineRevalidation, DeviceRevoked, UserDisabled, TenantSuspended/IsSuspended.
- Push idempotency: ProcessedSyncOperation. Pull ChangeLog: implemented (`change_log` + `tenant_change_counters`).
- Push outcomes: Applied / AlreadyProcessed / Conflict / Rejected / ValidationFailed. Tombstones = Deactivate.

### Roles

- Permissions GLOBAL (seeded). Roles TENANT-OWNED; system roles copied per tenant with `IsSystem = true`.

## Deferred implementation

See [architecture.md](architecture.md). Phase 1 does not create business tables or workflows.
