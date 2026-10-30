# Authorization

Status: **implemented** (Phase 2). Permission policies are registered; handlers/endpoints require permissions by code.

## Model

Authorization is permission-based. Roles are bundles. Handlers require a permission; they do not hard-code role names.

### Permissions — GLOBAL

Permissions are a global seeded catalog (`permissions` table). Codes are stable strings (for example `products.read`, `inventory.receive`). Tenants cannot invent unknown permission codes.

### Roles — TENANT-OWNED

Roles belong to a tenant (`tenant_id`, unique `(tenant_id, code)`).

**System roles are provisioned per tenant** when the tenant is created via `ITenantProvisioningService`:

- Copy the standard templates (Owner, Administrator, WarehouseManager, WarehouseWorker, SalesUser, PurchasingUser, Accountant, Viewer) into that tenant.
- Set `IsSystem = true` on those rows.
- Bind them to global permissions via `role_permissions` for that tenant.

A tenant may customize its own roles and role-permission links without modifying a global role row (there is no global role table).

Initial permission codes:

```text
products.read products.create products.update products.delete
inventory.read inventory.receive inventory.issue inventory.adjust inventory.transfer
purchases.create purchases.approve
sales.create sales.cancel
reports.read
users.manage
```

Warehouse scope is a second predicate (not yet enforced on routes). Assignment to Warehouse A does not grant Warehouse B.

## Enforcement

- JWT Bearer authentication.
- `TenantContextMiddleware` sets `ITenantContext` / `ICurrentUser` from claims (never from body/query/headers).
- Endpoints use `RequireAuthorization` and `RequirePermission("code")` (`perm:{code}` policies).
- Cross-tenant lookups return not-found. See [api.md](api.md).

`GET /api/v1/roles` and `GET /api/v1/permissions` require `users.manage`.

## Platform operators

Platform routes under `/api/v1/platform` require JWT claim `platform_operator=true` (policy `PlatformOperator`). They set an explicit tenant context when targeting a tenant and write audit rows. Tenant admins never receive platform operator privileges.

## Sync

At apply time the server rechecks user, device, permission, and warehouse scope.
