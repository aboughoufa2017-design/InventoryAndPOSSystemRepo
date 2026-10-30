# API strategy

Status: liveness, OpenAPI (Development), auth, catalog, inventory, transfers, purchasing, sales, sync, and **platform (SaaS)** routes are implemented.

## Versioning and shape

All business routes live under `/api/v1`. A breaking change ships as `/api/v2` or as a compatible migration with a documented overlap. Existing clients, including offline devices that sync on an old contract, are not broken silently.

```text
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh
POST   /api/v1/auth/logout
GET    /api/v1/auth/me
POST   /api/v1/devices/register
GET    /api/v1/roles
GET    /api/v1/permissions
GET    /api/v1/users
POST   /api/v1/users
GET    /api/v1/users/{id}
PUT    /api/v1/users/{id}
POST   /api/v1/users/{id}/set-active
POST   /api/v1/users/{id}/reset-password

/api/v1/products
/api/v1/warehouses
/api/v1/inventory
/api/v1/purchases
/api/v1/sales
/api/v1/transfers
/api/v1/sync/push
/api/v1/sync/pull

/api/v1/platform/...    SaaS operator only (Phase 11 — see [saas-platform.md](saas-platform.md))
```

Auth, catalog, inventory, transfers, purchasing, sales, sync, and platform routes above are implemented.

```text
GET/POST /api/v1/units
GET/POST /api/v1/categories
PUT      /api/v1/categories/{id}
GET/POST /api/v1/products
GET      /api/v1/products/{id}
GET      /api/v1/products/by-sku/{sku}
GET      /api/v1/products/by-barcode/{barcode}
PUT      /api/v1/products/{id}
POST     /api/v1/products/{id}/barcodes
DELETE   /api/v1/products/{id}/barcodes/{barcodeId}
```

Product permissions: `products.read` / `create` / `update` / `delete`.

Inventory (Phase 4):

```text
GET/POST /api/v1/warehouses
GET      /api/v1/warehouses/{id}/locations
GET      /api/v1/inventory/balances
GET      /api/v1/inventory/movements
POST     /api/v1/inventory/adjustments
POST     /api/v1/inventory/adjustments/{id}/post
POST     /api/v1/inventory/adjustments/{id}/cancel
```

Transfers (Phase 5):

```text
GET/POST /api/v1/transfers
POST     /api/v1/transfers/{id}/dispatch
POST     /api/v1/transfers/{id}/receive
POST     /api/v1/transfers/{id}/cancel
```

Purchasing (Phase 6):

```text
GET/POST /api/v1/suppliers
GET/PUT  /api/v1/suppliers/{id}
GET/POST /api/v1/purchases
POST     /api/v1/purchases/{id}/confirm
POST     /api/v1/purchases/{id}/receive
POST     /api/v1/purchases/{id}/returns
POST     /api/v1/purchases/{id}/close
POST     /api/v1/purchases/{id}/cancel
PUT      /api/v1/purchases/{id}/payment
```

Sales (Phase 7):

```text
GET/POST /api/v1/customers
GET/PUT  /api/v1/customers/{id}
GET/POST /api/v1/sales
POST     /api/v1/sales/{id}/confirm
POST     /api/v1/sales/{id}/ship
POST     /api/v1/sales/{id}/returns
POST     /api/v1/sales/{id}/close
POST     /api/v1/sales/{id}/cancel
PUT      /api/v1/sales/{id}/payment
```

Sync (Phase 8):

```text
POST /api/v1/sync/push
POST /api/v1/sync/pull
```

JSON property names are camelCase. Timestamps are UTC ISO-8601. Identifiers are UUID strings. Money and quantities are JSON numbers produced from `decimal` (not binary floating-point fields in .NET).

## Responses and errors

Success responses are the resource or a paged envelope:

```json
{
  "items": [],
  "nextCursor": null
}
```

Errors use `ApiErrorResponse`:

```json
{
  "code": "INSUFFICIENT_STOCK",
  "message": "Not enough stock to post this sale.",
  "correlationId": "3f1c...",
  "errors": [{ "field": "lines[0].quantity", "message": "Quantity must be greater than zero." }]
}
```

`errors` is omitted when there is no field list. Production bodies never include stack traces, SQL, or secrets. The correlation id is also returned in the `X-Correlation-ID` response header. Clients may send that header; values must be 1–64 characters from `A-Z`, `a-z`, `0-9`, `-`, and `_`. Anything else is replaced with a new UUID.

Stable codes live in `Warehouse.Shared.Api.ApiErrorCodes`:

| Code | Meaning |
| --- | --- |
| `PRODUCT_NOT_FOUND` | Missing or not visible in this tenant |
| `INSUFFICIENT_STOCK` | Posting would break the negative-stock policy |
| `UNAUTHORIZED` | Missing or invalid authentication |
| `FORBIDDEN` | Authenticated, not allowed |
| `TENANT_NOT_FOUND` | Platform target tenant does not exist |
| `INVALID_OPERATION` | The document state does not allow the action |
| `SYNC_CONFLICT` | A mutable row lost the version check |
| `OPERATION_ALREADY_PROCESSED` | Returned with the original result; not an error the client should retry as new work |
| `VALIDATION_FAILED` | Input failed FluentValidation |
| `INTERNAL_ERROR` | Unexpected failure |

`OPERATION_ALREADY_PROCESSED` is a success replay for the caller. The HTTP status for a replay is the original status, not a new failure. The code is present so logs and clients can tell a replay from a first apply.

Cross-tenant entity access uses `404` with a not-found code for that resource. It does not use a distinct "wrong tenant" code.

## Pagination, idempotency, and sync body

Large collections accept a cursor and a limit. The default limit is small (proposed 50, maximum 200). Limit and default become configuration when the first list endpoint exists.

Commands that must be safe to retry accept either:

- `Idempotency-Key` header for ordinary commands, or
- `operationId` in the sync push body.

The server treats that id as the idempotency key. Sync push returns one result per operation so a failure does not hide the rest of the batch. See [synchronization.md](synchronization.md).

## OpenAPI and the current host

OpenAPI is generated by Swashbuckle and is enabled only when `ASPNETCORE_ENVIRONMENT` is Development. The document includes health, auth, catalog, inventory, purchasing, sales, transfers, sync, users, and platform routes.

`GET /health/live` returns `{ "status": "Healthy" }` when the process is up. It does not check dependencies.

`GET /health/ready` checks PostgreSQL and Redis and returns `{ "status", "checks" }` without exception text. **200** when healthy, **503** otherwise.

Unhandled exceptions return HTTP 500 and `INTERNAL_ERROR`. The exception is logged with the correlation id. The client does not receive the exception message.
