using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Security;
using Warehouse.Application.Sync;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Sync;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Sync.Handlers;

public sealed class ProductUpsertSyncHandler(
    WarehouseDbContext db,
    Application.Tenancy.ITenantContext tenantContext,
    ICurrentUser currentUser) : ISyncOperationHandler
{
    public string EntityType => SyncEntityTypes.Product;

    public string OperationType => SyncOperationTypes.Upsert;

    public async Task<SyncHandlerResult> HandleAsync(SyncPushOperationRequest operation, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return Reject(SyncPushOutcomes.Rejected, ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (!currentUser.HasPermission("products.create") && !currentUser.HasPermission("products.update"))
        {
            return Reject(SyncPushOutcomes.Rejected, ErrorCodes.Forbidden, "Missing products.create or products.update.");
        }

        ProductUpsertPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ProductUpsertPayload>(operation.PayloadJson, SyncJson.Options);
        }
        catch (JsonException)
        {
            return Reject(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Invalid product payload.");
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Sku) || string.IsNullOrWhiteSpace(payload.Name))
        {
            return Reject(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Sku and name are required.");
        }

        var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == operation.EntityId, cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;

        try
        {
            if (existing is null)
            {
                if (!await db.UnitsOfMeasure.AnyAsync(u => u.Id == payload.UnitOfMeasureId, cancellationToken))
                {
                    return Reject(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Unit of measure was not found.");
                }

                var sku = payload.Sku.Trim().ToUpperInvariant();
                if (await db.Products.AnyAsync(p => p.Sku == sku, cancellationToken))
                {
                    return Reject(SyncPushOutcomes.Conflict, ErrorCodes.SyncConflict, "Product SKU already exists.");
                }

                var product = Product.Create(
                    operation.EntityId,
                    tenantId,
                    payload.Sku,
                    payload.Name,
                    payload.UnitOfMeasureId,
                    payload.PurchasePrice,
                    payload.SalePrice,
                    payload.IsTaxable,
                    payload.TrackInventory,
                    payload.CategoryId,
                    payload.Description,
                    payload.Brand,
                    payload.MinimumStock,
                    payload.MaximumStock,
                    payload.ReorderPoint,
                    utcNow);
                db.Products.Add(product);
                var createdJson = JsonSerializer.Serialize(Map(product), SyncJson.Options);
                return new SyncHandlerResult(
                    SyncPushOutcomes.Applied,
                    null,
                    null,
                    createdJson,
                    ChangeLogOperation.Upsert,
                    createdJson);
            }

            if (operation.BaseVersion is int expected && existing.RowVersion != expected)
            {
                return Reject(SyncPushOutcomes.Conflict, ErrorCodes.SyncConflict, "Product was modified by another request.");
            }

            existing.Update(
                payload.Name,
                payload.UnitOfMeasureId,
                payload.PurchasePrice,
                payload.SalePrice,
                payload.IsTaxable,
                payload.TrackInventory,
                payload.CategoryId,
                payload.Description,
                payload.Brand,
                payload.MinimumStock,
                payload.MaximumStock,
                payload.ReorderPoint,
                utcNow);
            if (payload.IsActive is bool isActive)
            {
                existing.SetActive(isActive, utcNow);
            }

            var updatedJson = JsonSerializer.Serialize(Map(existing), SyncJson.Options);
            return new SyncHandlerResult(
                SyncPushOutcomes.Applied,
                null,
                null,
                updatedJson,
                ChangeLogOperation.Upsert,
                updatedJson);
        }
        catch (ArgumentException ex)
        {
            return Reject(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message == "SYNC_CONFLICT")
        {
            return Reject(SyncPushOutcomes.Conflict, ErrorCodes.SyncConflict, "Product was modified by another request.");
        }
    }

    private static object Map(Product p) => new
    {
        p.Id,
        p.Sku,
        p.Name,
        p.Description,
        p.Brand,
        p.CategoryId,
        p.UnitOfMeasureId,
        p.PurchasePrice,
        p.SalePrice,
        p.IsTaxable,
        p.IsActive,
        p.TrackInventory,
        p.MinimumStock,
        p.MaximumStock,
        p.ReorderPoint,
        p.RowVersion,
    };

    private static SyncHandlerResult Reject(string outcome, string code, string message) =>
        new(outcome, code, message, JsonSerializer.Serialize(new { code, message }, SyncJson.Options));

    private sealed record ProductUpsertPayload(
        string Sku,
        string Name,
        Guid UnitOfMeasureId,
        decimal PurchasePrice,
        decimal SalePrice,
        bool IsTaxable,
        bool TrackInventory,
        Guid? CategoryId,
        string? Description,
        string? Brand,
        decimal? MinimumStock,
        decimal? MaximumStock,
        decimal? ReorderPoint,
        bool? IsActive);
}

public sealed class ProductDeactivateSyncHandler(WarehouseDbContext db, Application.Tenancy.ITenantContext tenantContext)
    : ISyncOperationHandler
{
    public string EntityType => SyncEntityTypes.Product;

    public string OperationType => SyncOperationTypes.Deactivate;

    public async Task<SyncHandlerResult> HandleAsync(SyncPushOperationRequest operation, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return new SyncHandlerResult(
                SyncPushOutcomes.Rejected,
                ErrorCodes.Unauthorized,
                "Not authenticated.",
                "{}");
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == operation.EntityId, cancellationToken);
        if (product is null)
        {
            return new SyncHandlerResult(
                SyncPushOutcomes.ValidationFailed,
                ErrorCodes.ProductNotFound,
                "Product was not found.",
                "{}");
        }

        if (operation.BaseVersion is int expected && product.RowVersion != expected)
        {
            return new SyncHandlerResult(
                SyncPushOutcomes.Conflict,
                ErrorCodes.SyncConflict,
                "Product was modified by another request.",
                "{}");
        }

        product.SetActive(false, DateTimeOffset.UtcNow);
        var payload = JsonSerializer.Serialize(new { product.Id, product.IsActive, product.RowVersion }, SyncJson.Options);
        return new SyncHandlerResult(
            SyncPushOutcomes.Applied,
            null,
            null,
            payload,
            ChangeLogOperation.Deactivate,
            payload);
    }
}
