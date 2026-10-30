using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Pos;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Pos;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Pos;

public sealed class RestaurantPosService(
    WarehouseDbContext db,
    ITenantContext tenantContext) : IRestaurantPosService
{
    public async Task<AppResult<RestaurantTableResponse>> CreateTableAsync(
        CreateRestaurantTableCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<RestaurantTableResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            return AppResult<RestaurantTableResponse>.Failure(ErrorCodes.ValidationFailed, "Table code is required.");
        }

        if (!await db.Warehouses.AnyAsync(w => w.Id == command.WarehouseId, cancellationToken))
        {
            return AppResult<RestaurantTableResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var code = command.Code.Trim().ToUpperInvariant();
        if (await db.Set<RestaurantTable>().AnyAsync(
                t => t.TenantId == tenantId && t.WarehouseId == command.WarehouseId && t.Code == code,
                cancellationToken))
        {
            return AppResult<RestaurantTableResponse>.Failure(ErrorCodes.ValidationFailed, "Table code already exists.");
        }

        try
        {
            var table = RestaurantTable.Create(Guid.NewGuid(), tenantId, command.WarehouseId, code, command.Name);
            db.Set<RestaurantTable>().Add(table);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<RestaurantTableResponse>.Success(Map(table));
        }
        catch (ArgumentException ex)
        {
            return AppResult<RestaurantTableResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (DbUpdateException)
        {
            return AppResult<RestaurantTableResponse>.Failure(ErrorCodes.ValidationFailed, "Table code already exists.");
        }
    }

    public async Task<IReadOnlyList<RestaurantTableResponse>> ListTablesAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }

        var tables = await db.Set<RestaurantTable>().AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.WarehouseId == warehouseId)
            .OrderBy(t => t.Code)
            .ToListAsync(cancellationToken);
        return tables.Select(Map).ToList();
    }

    public async Task<AppResult<RestaurantOrderResponse>> OpenOrderAsync(
        OpenRestaurantOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<RestaurantOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var table = await db.Set<RestaurantTable>().AsNoTracking().FirstOrDefaultAsync(
            t => t.TenantId == tenantId
                 && t.Id == command.TableId
                 && t.WarehouseId == command.WarehouseId
                 && t.IsActive,
            cancellationToken);
        if (table is null)
        {
            return AppResult<RestaurantOrderResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Table was not found, is inactive, or belongs to another warehouse.");
        }

        if (await db.Set<RestaurantOpenOrder>().AnyAsync(
                o => o.TenantId == tenantId && o.TableId == table.Id && o.Status == RestaurantOrderStatus.Open,
                cancellationToken))
        {
            return AppResult<RestaurantOrderResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "This table already has an open order.");
        }

        var order = RestaurantOpenOrder.Open(
            Guid.NewGuid(),
            tenantId,
            command.WarehouseId,
            table.Id,
            command.Notes,
            DateTimeOffset.UtcNow);
        db.Set<RestaurantOpenOrder>().Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<RestaurantOrderResponse>.Success(Map(order));
    }

    public async Task<IReadOnlyList<RestaurantOrderResponse>> ListOpenOrdersAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }

        var orders = await db.Set<RestaurantOpenOrder>().AsNoTracking()
            .Where(o => o.TenantId == tenantId
                        && o.WarehouseId == warehouseId
                        && o.Status == RestaurantOrderStatus.Open)
            .OrderBy(o => o.OpenedAtUtc)
            .ToListAsync(cancellationToken);
        return orders.Select(Map).ToList();
    }

    public async Task<AppResult<RestaurantOrderResponse>> CancelOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<RestaurantOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var order = await db.Set<RestaurantOpenOrder>()
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == orderId, cancellationToken);
        if (order is null)
        {
            return AppResult<RestaurantOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Order was not found.");
        }

        try
        {
            order.Cancel(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<RestaurantOrderResponse>.Success(Map(order));
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<RestaurantOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    private static RestaurantTableResponse Map(RestaurantTable t) =>
        new(t.Id, t.WarehouseId, t.Code, t.Name, t.IsActive);

    private static RestaurantOrderResponse Map(RestaurantOpenOrder o) =>
        new(o.Id, o.WarehouseId, o.TableId, o.Status.ToString(), o.Notes, o.OpenedAtUtc, o.PosSaleId);
}
