using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Pos;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Pos;
using Warehouse.Domain.Precision;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Pos;

public sealed class PosPricingService(
    WarehouseDbContext db,
    ITenantContext tenantContext) : IPosPricingService
{
    public async Task<AppResult<PriceListResponse>> CreatePriceListAsync(
        CreatePriceListCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PriceListResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Name))
        {
            return AppResult<PriceListResponse>.Failure(ErrorCodes.ValidationFailed, "Code and name are required.");
        }

        var code = command.Code.Trim().ToUpperInvariant();
        if (await db.Set<PriceList>().AnyAsync(p => p.TenantId == tenantId && p.Code == code, cancellationToken))
        {
            return AppResult<PriceListResponse>.Failure(ErrorCodes.ValidationFailed, "Price list code already exists.");
        }

        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        try
        {
            var list = PriceList.Create(
                Guid.NewGuid(),
                tenantId,
                code,
                command.Name,
                string.IsNullOrWhiteSpace(command.CurrencyCode) ? tenant.CurrencyCode : command.CurrencyCode,
                DateTimeOffset.UtcNow);
            db.Set<PriceList>().Add(list);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<PriceListResponse>.Success(Map(list));
        }
        catch (ArgumentException ex)
        {
            return AppResult<PriceListResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (DbUpdateException)
        {
            return AppResult<PriceListResponse>.Failure(ErrorCodes.ValidationFailed, "Price list code already exists.");
        }
    }

    public async Task<IReadOnlyList<PriceListResponse>> ListPriceListsAsync(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }

        var lists = await db.Set<PriceList>().AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
        return lists.Select(Map).ToList();
    }

    public async Task<AppResult> SetProductPriceAsync(
        SetProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.UnitPrice < 0m || command.MinQuantity is <= 0m)
        {
            return AppResult.Failure(
                ErrorCodes.ValidationFailed,
                "Unit price cannot be negative and minimum quantity must be positive.");
        }

        if (!await db.Set<PriceList>().AnyAsync(
                p => p.TenantId == tenantId && p.Id == command.PriceListId && p.IsActive,
                cancellationToken))
        {
            return AppResult.Failure(ErrorCodes.ValidationFailed, "Price list was not found or is inactive.");
        }

        if (!await db.Products.AnyAsync(p => p.Id == command.ProductId && p.IsActive, cancellationToken))
        {
            return AppResult.Failure(ErrorCodes.ProductNotFound, "Product was not found or is inactive.");
        }

        try
        {
            var minQty = command.MinQuantity is null
                ? (decimal?)null
                : Math.Round(command.MinQuantity.Value, QuantityPrecision.Scale, MidpointRounding.AwayFromZero);

            // One price row per (list, product, min quantity): replace an existing tier instead of stacking duplicates.
            var existing = await db.Set<ProductPrice>()
                .Where(p => p.TenantId == tenantId
                            && p.PriceListId == command.PriceListId
                            && p.ProductId == command.ProductId
                            && p.MinQuantity == minQty)
                .ToListAsync(cancellationToken);
            if (existing.Count > 0)
            {
                db.Set<ProductPrice>().RemoveRange(existing);
            }

            db.Set<ProductPrice>().Add(ProductPrice.Create(
                Guid.NewGuid(),
                tenantId,
                command.PriceListId,
                command.ProductId,
                command.UnitPrice,
                minQty));
            await db.SaveChangesAsync(cancellationToken);
            return AppResult.Success();
        }
        catch (ArgumentException ex)
        {
            return AppResult.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
    }

    public async Task<decimal?> ResolveUnitPriceAsync(
        Guid productId,
        Guid? priceListId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        if (priceListId is not Guid listId || tenantContext.TenantId is not Guid tenantId)
        {
            return null;
        }

        var listActive = await db.Set<PriceList>().AsNoTracking()
            .AnyAsync(p => p.TenantId == tenantId && p.Id == listId && p.IsActive, cancellationToken);
        if (!listActive)
        {
            return null;
        }

        // Highest MinQuantity tier that the requested quantity still satisfies (null min = base tier).
        var tiers = await db.Set<ProductPrice>().AsNoTracking()
            .Where(p => p.TenantId == tenantId
                        && p.PriceListId == listId
                        && p.ProductId == productId
                        && (p.MinQuantity == null || p.MinQuantity <= quantity))
            .ToListAsync(cancellationToken);

        return tiers
            .OrderByDescending(p => p.MinQuantity ?? 0m)
            .Select(p => (decimal?)p.UnitPrice)
            .FirstOrDefault();
    }

    private static PriceListResponse Map(PriceList p) =>
        new(p.Id, p.Code, p.Name, p.CurrencyCode, p.IsActive);
}
