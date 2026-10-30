# Inventory Integration Map (POS ↔ Warehouse)

Status: **Phase 0 complete**. Shared PostgreSQL. No second product catalog. No second stock ledger.

## Decision

| Decision | Choice |
| --- | --- |
| Database | Same PostgreSQL as Warehouse (already true) |
| Branch | POS `branch_id` = existing `WarehouseId` |
| Catalog | Reuse `Product`, `ProductBarcode`, `UnitOfMeasure`, `SalePrice` |
| Stock writes | Only via inventory application services / POS checkout that applies the same movement rules as sales ship |
| Sale model | New `PosSale` aggregate (not `SalesOrder`) — atomic ticket + cash tender |
| Transaction | Finalize sale + stock movements in one EF `SaveChanges` / ambient transaction |

## Map

| POS feature | Existing entity / table | Existing service / API | Ownership | Gap |
| --- | --- | --- | --- | --- |
| Product lookup | `Product`, `ProductBarcode` | `ICatalogService.GetProductByBarcodeAsync` · `GET /api/v1/products/by-barcode/{barcode}` | Catalog | Thin POS lookup endpoint OK |
| Price | `Product.SalePrice` | Catalog | Catalog | Price lists deferred (Phase 5+) |
| UoM / fractional | `UnitOfMeasure` | Catalog | Catalog | Grocery fractions later |
| Stock on hand | `StockBalance` | `IInventoryService.GetBalanceAsync` | Inventory | Display stale on offline clients |
| Issue stock on sale | `StockMovement` `SalesShipment` | Today: `SalesService.ShipAsync` | Inventory rules via POS adapter | POS must call shared posting path, not raw balances |
| Negative stock | `Tenant.NegativeStockPolicy` | `NegativeStockEvaluator` | Tenancy + Inventory | Reuse with `inventory.issue` |
| B2B orders | `SalesOrder` | Sales APIs | Sales | Keep separate from POS tickets |
| Customer | `Customer` | Customers API | Sales | POS cash: walk-in / nullable customer |
| Device | `Device` | `POST /api/v1/devices/register` | Devices | Add optional `WarehouseId` (branch binding) |
| Sync idempotency | `ProcessedSyncOperation` | `SyncService` | Sync | Add `PosSale` / `Finalize` handler |
| Online idempotency | `SalesOperation` pattern | Sales | POS | `ClientCommandId` on `PosSale` |
| Entitlements | `sales` feature | `RequireSales` | Platform | Reuse `sales` for MVP; optional `pos` later |
| Permissions | `sales.create`, `inventory.issue` | SystemPermissions | Identity | Add `pos.sale`, `pos.void`, `pos.refund`, `pos.shift` |
| Cash shifts | — | — | POS | New tables Phase 5 |
| Conflicts | — | — | POS | `pos_inventory_conflicts` Phase 3 |
| Reports | Report endpoints | Reporting | Reporting | POS sales summary Phase 5 |

## New tables only (no catalog/ledger duplicates)

| Table | Phase |
| --- | --- |
| `pos_sales` | 2 |
| `pos_sale_lines` | 2 |
| `pos_payments` | 2 |
| `pos_inventory_conflicts` | 3 |
| `cash_shifts` / `cash_movements` | 5 |
| `price_lists` / `product_prices` | 5+ |

## Regression tests required before/with checkout

- Opening balance / adjustment post still works
- SalesOrder ship still posts `SalesShipment` once
- Tenant isolation on products/balances
- POS finalize: OnHand decreases once; same `ClientCommandId` replays without second movement

## Explicit non-duplication

- Do **not** create `pos_products` or `pos_stock_balances`
- Do **not** bypass `NegativeStockEvaluator` / movement ledger
- Warehouse `SalesOrder` remains for B2B order→ship; POS is retail tickets
