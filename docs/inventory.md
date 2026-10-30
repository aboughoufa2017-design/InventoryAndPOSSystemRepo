# Inventory

Status: **implemented** (Phase 4).

## Model

- **StockBalance** — projection: Tenant + Product + Warehouse + LocationId (NOT NULL) + OnHand + Reserved. **Available = OnHand − Reserved** (calculated, not stored).
- **StockMovement** — immutable ledger. Signed `QuantityChange`. Optional `TransferLineId`, `SourceDocumentType`, `SourceDocumentId`.
- **StockAdjustment** — Draft → Posted/Cancelled. Kinds: `OpeningBalance`, `Correction`. Stock changes only on **Posted**.

## Negative stock

Tenant `NegativeStockPolicy`:

| Policy | Behavior |
| --- | --- |
| Prevent | Reject resulting OnHand &lt; 0 |
| Allow | Permit negative OnHand |
| AllowWithPermission | Permit negative only when caller has `inventory.adjust` |

Do not add Warn.

## API

- `GET/POST /api/v1/warehouses` (creates GENERAL + IN_TRANSIT locations)
- `GET /api/v1/warehouses/{id}/locations`
- `GET /api/v1/inventory/balances`
- `GET /api/v1/inventory/movements`
- `POST /api/v1/inventory/adjustments` (+ post / cancel)

Permissions: `inventory.read`, `inventory.adjust`.
