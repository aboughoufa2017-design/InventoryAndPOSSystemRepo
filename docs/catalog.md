# Product catalog

Status: **implemented** (Phase 3).

## Model

Tenant-owned:

- **UnitOfMeasure** — codes such as PIECE, BOX, CARTON, KG, G, L, M (system-seeded on tenant provision; tenants may add more)
- **ProductCategory** — optional hierarchy via `ParentCategoryId`
- **Product** — one SKU per product (`Sku` unique per tenant)
- **ProductBarcode** — multiple barcodes per product; barcode unique per tenant

Prices use `numeric(19,4)`. Stock thresholds use `numeric(18,6)`.

Products are soft-deactivated (`IsActive`); do not hard-delete when inventory history will exist.

## Tax integration

- Tenant tax defaults to **disabled**.
- Product `IsTaxable` participates only when tenant tax is enabled.
- `ProductTaxCalculator.ResolveApplicableRate` / `CalculateTaxAmount` apply tenant `TaxRate` (fraction of 1).
- Inventory posting never applies tax.

## API

See [api.md](api.md). Requires product permissions.
