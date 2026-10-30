# Conflict resolution

Status: **implemented** with sync push outcomes (Phase 8). Last-write-wins is not the inventory strategy.

Last-write-wins is not the inventory strategy. Two offline sales are two events. Both apply unless a business rule rejects one.

## Inventory

Device A and device B both start from OnHand 100 (Available 100 if Reserved is 0).

- A sells 20 offline.
- B sells 30 offline.

After both sync, OnHand is 50. Each posts its own stock movements.

If policy is Prevent and stock is insufficient, the second operation is rejected with `INSUFFICIENT_STOCK`. No silent clamping.

### Transfers and IN_TRANSIT

- Dispatch: source OnHand ↓, `IN_TRANSIT` OnHand ↑.
- Receive: `IN_TRANSIT` OnHand ↓, destination OnHand ↑.
- Partial receive supported; OperationId idempotent.
- TransferLine is the document of record; StockBalance is always by LocationId.

Tax does not participate in inventory quantity conflicts.

## Mutable master data

Integer `row_version` optimistic concurrency → `SYNC_CONFLICT` on stale update.

## Documents

State conflicts return `INVALID_OPERATION`. Payment status uses Unpaid / PartiallyPaid / Paid when those modules exist. No general-ledger conflicts in v1.

Generic lifecycle: Draft / Approved / Posted / Cancelled. Module-specific purchasing/sales states are separate.

## Permissions and device security

Lost permission, revoked device, disabled user, or suspended tenant → reject at apply; not a transient retry. Flags apply on next connect. Offline window default is MaxOfflineDays = 7 (tenant-configurable).

## Local merge

Pull must not drop unacknowledged outbox rows. Push first; then version checks apply.

Sync push returns explicit outcomes: Applied, AlreadyProcessed, Conflict, Rejected, ValidationFailed.
