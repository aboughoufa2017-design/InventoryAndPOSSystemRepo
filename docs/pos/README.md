# POS module

Status: **implemented** (retail P0 + Phase 5 increments). Extends Warehouse; see [inventory-integration-map.md](inventory-integration-map.md).

## Online checkout

- Web: `/pos` — scan/SKU cart → cash/credit → receipt print
- API: `POST /api/v1/pos/sales` with `clientCommandId` (idempotent)
- Stock: `SalesShipment` movements via inventory rules; conflicts logged when OnHand goes negative under allow policies

## Offline

- Flutter: POS checkout screen queues `PosSale` / `Finalize` to outbox
- Sync handler: `PosSaleFinalizeSyncHandler`

## Also included

| Feature | Routes / notes |
| --- | --- |
| Cash shifts | `/api/v1/pos/shifts` |
| Void / refund | `POST .../void`, `.../refund` |
| Inventory conflicts | `/api/v1/pos/conflicts` |
| Price lists | `/api/v1/pos/price-lists` (quantity breaks) |
| Restaurant tables / open tickets | `/api/v1/pos/restaurants/*` (minimal) |
| Device branch binding | `WarehouseId` on device register |

## Permissions

`pos.sale`, `pos.void`, `pos.refund`, `pos.shift` (+ `inventory.issue` for stock). Role **Cashier** seeded for new tenants.
