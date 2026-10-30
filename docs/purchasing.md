# Purchasing

Status: **implemented** (Phase 6).

## Concepts

- **Supplier** — master data (`code` unique per tenant)
- **PurchaseOrder** / **PurchaseOrderLine** — commercial document with money (`numeric(19,4)`) and quantities (`numeric(18,6)`)
- **PurchaseReceipt** / **PurchaseReceiptLine** — goods receipt; posts stock
- **PurchaseReturn** / **PurchaseReturnLine** — return of received stock; posts compensating movements
- **PaymentStatus** — `Unpaid` / `PartiallyPaid` / `Paid` (no general ledger)

## Lifecycle

```text
Draft → Confirmed → PartiallyReceived → FullyReceived → Closed
                 ↘ Cancelled (Draft or Confirmed with zero receipts)
```

**Confirm does not change stock.** Receiving increases OnHand; returns decrease OnHand. Both create immutable `StockMovement` rows (`PurchaseReceipt` / `PurchaseReturn`).

## Idempotency and concurrency

- Optional `operationId` on confirm / receive / return / close / cancel / payment → `purchase_operations` (PK = operationId). Replay returns prior result with `wasIdempotentReplay: true`.
- `expectedRowVersion` on mutating commands (`SYNC_CONFLICT` on mismatch).

## API

```text
GET/POST /api/v1/suppliers
GET/PUT  /api/v1/suppliers/{id}

GET/POST /api/v1/purchases
GET      /api/v1/purchases/{id}
POST     /api/v1/purchases/{id}/confirm
POST     /api/v1/purchases/{id}/receive
POST     /api/v1/purchases/{id}/returns
POST     /api/v1/purchases/{id}/close
POST     /api/v1/purchases/{id}/cancel
PUT      /api/v1/purchases/{id}/payment
```

Permissions: `purchases.create` (suppliers + create/list/get), `purchases.approve` (confirm/return/close/cancel/payment), `inventory.receive` (receive).

Tax on lines uses `ProductTaxCalculator` (tenant tax disabled ⇒ rate 0).
