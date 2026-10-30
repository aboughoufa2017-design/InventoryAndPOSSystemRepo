# Transfers

Status: **implemented** (Phase 5).

## Flow

```text
Source location  →  source warehouse IN_TRANSIT  →  destination location
```

1. **Create** (Draft) — no stock effect
2. **Dispatch** — source OnHand ↓, source `IN_TRANSIT` OnHand ↑; creates StockMovements with `TransferLineId`
3. **Receive** (partial allowed) — `IN_TRANSIT` OnHand ↓, destination OnHand ↑
4. **Cancel** — Draft: cancel only; Dispatched/PartiallyReceived: return remaining in-transit to source

Statuses: Draft → Dispatched → PartiallyReceived → Completed (or Cancelled).

## Idempotency and concurrency

- Optional `operationId` on dispatch/receive/cancel stored in `transfer_operations` (PK = operationId). Replay returns prior result with `wasIdempotentReplay: true`.
- `expectedRowVersion` required on mutating commands (`SYNC_CONFLICT` on mismatch).

## API

```text
GET/POST /api/v1/transfers
POST     /api/v1/transfers/{id}/dispatch
POST     /api/v1/transfers/{id}/receive
POST     /api/v1/transfers/{id}/cancel
```

Permission: `inventory.transfer` (mutations), `inventory.read` (list/get).
