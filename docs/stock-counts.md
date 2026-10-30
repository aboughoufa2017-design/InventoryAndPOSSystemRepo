# Stock counts

Status: **implemented**.

Physical inventory counts at a warehouse location. Creating a count seeds lines from active products that track inventory, using the current system `OnHand` as `SystemQuantity`. Entering counted quantities and posting applies non-zero variances as a **Correction** stock adjustment through the existing inventory engine (no duplicate stock math).

## Lifecycle

`Draft` → `Counted` (after lines saved) → `Posted` or `Cancelled`.

## API

```text
GET    /api/v1/stock-counts
POST   /api/v1/stock-counts
GET    /api/v1/stock-counts/{id}
PUT    /api/v1/stock-counts/{id}/lines
POST   /api/v1/stock-counts/{id}/post
POST   /api/v1/stock-counts/{id}/cancel
```

Requires permission `inventory.adjust` and entitlement feature `inventory`.

## Web

Tenant app route `/stock-counts`.
