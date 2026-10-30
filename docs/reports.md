# Reports

Status: **implemented**.

SQL/EF queries over operational tables (no separate BI warehouse).

## Endpoints

All require permission `reports.read` and entitlement feature `reports`.

| Route | Purpose |
| --- | --- |
| `GET /api/v1/reports/stock-on-hand?warehouseId=` | On-hand / reserved / available by product & location |
| `GET /api/v1/reports/movements?fromUtc=&toUtc=&warehouseId=` | Movement history |
| `GET /api/v1/reports/purchases?fromUtc=&toUtc=` | Purchase order summary |
| `GET /api/v1/reports/sales?fromUtc=&toUtc=` | Sales order summary |

## Web

Tenant route `/reports`: date filters, load each report, **CSV download**, and browser **print**.
