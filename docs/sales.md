# Sales

Status: **implemented** (Phase 7).

## Concepts

- **Customer** — master data (`code` unique per tenant)
- **SalesOrder** / **SalesOrderLine** — commercial document with money (`numeric(19,4)`) and quantities (`numeric(18,6)`)
- **SalesShipment** / **SalesShipmentLine** — fulfillment; posts stock out
- **SalesReturn** / **SalesReturnLine** — return of shipped stock; posts compensating movements
- **PaymentStatus** — `Unpaid` / `PartiallyPaid` / `Paid` (no general ledger)

## Lifecycle

```text
Draft → Confirmed → PartiallyFulfilled → Fulfilled → Closed
                 ↘ Cancelled (Draft or Confirmed with zero shipments)
```

**Confirm does not change stock.** Shipping decreases OnHand; returns increase OnHand. Both create immutable `StockMovement` rows (`SalesShipment` / `SalesReturn`).

## Idempotency and concurrency

- Optional `operationId` on confirm / ship / return / close / cancel / payment → `sales_operations` (PK = operationId). Replay returns prior result with `wasIdempotentReplay: true`.
- `expectedRowVersion` on mutating commands (`SYNC_CONFLICT` on mismatch).

## API

```text
GET/POST /api/v1/customers
GET/PUT  /api/v1/customers/{id}

GET/POST /api/v1/sales
GET      /api/v1/sales/{id}
POST     /api/v1/sales/{id}/confirm
POST     /api/v1/sales/{id}/ship
POST     /api/v1/sales/{id}/returns
POST     /api/v1/sales/{id}/close
POST     /api/v1/sales/{id}/cancel
PUT      /api/v1/sales/{id}/payment
```

Permissions: `sales.create` (customers + create/list/get/confirm/return/close/payment), `inventory.issue` (ship), `sales.cancel` (cancel).

Tax on lines uses `ProductTaxCalculator` (tenant tax disabled ⇒ rate 0). Default unit price is `Product.SalePrice`.
