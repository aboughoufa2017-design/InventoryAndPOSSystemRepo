using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Pos;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Pos;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Pos;

public sealed class CashShiftService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : ICashShiftService
{
    public async Task<AppResult<CashShiftResponse>> OpenAsync(
        OpenCashShiftCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId || currentUser.UserId is not Guid cashierId)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OpeningFloat < 0m)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.ValidationFailed, "Opening float cannot be negative.");
        }

        if (!await db.Warehouses.AnyAsync(w => w.Id == command.WarehouseId, cancellationToken))
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var hasOpen = await db.Set<CashShift>().AnyAsync(
            s => s.TenantId == tenantId
                 && s.WarehouseId == command.WarehouseId
                 && s.CashierUserId == cashierId
                 && s.Status == CashShiftStatus.Open,
            cancellationToken);
        if (hasOpen)
        {
            return AppResult<CashShiftResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "An open cash shift already exists for this cashier and warehouse.");
        }

        try
        {
            var shift = CashShift.Open(
                Guid.NewGuid(),
                tenantId,
                command.WarehouseId,
                cashierId,
                command.OpeningFloat,
                DateTimeOffset.UtcNow,
                command.DeviceId);

            db.Set<CashShift>().Add(shift);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<CashShiftResponse>.Success(Map(shift));
        }
        catch (ArgumentException ex)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
    }

    public async Task<AppResult<CashShiftResponse>> CloseAsync(
        CloseCashShiftCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.CountedCash < 0m)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.ValidationFailed, "Counted cash cannot be negative.");
        }

        var shift = await db.Set<CashShift>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == command.CashShiftId, cancellationToken);
        if (shift is null)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.InvalidOperation, "Cash shift was not found.");
        }

        try
        {
            shift.Close(command.CountedCash, DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<CashShiftResponse>.Success(Map(shift));
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return AppResult<CashShiftResponse>.Failure(ErrorCodes.SyncConflict, "Cash shift was modified by another request.");
        }
    }

    public async Task<IReadOnlyList<CashShiftResponse>> ListAsync(Guid? warehouseId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }

        var query = db.Set<CashShift>().AsNoTracking().Where(s => s.TenantId == tenantId);
        if (warehouseId is Guid wid)
        {
            query = query.Where(s => s.WarehouseId == wid);
        }

        var shifts = await query.OrderByDescending(s => s.OpenedAtUtc).Take(200).ToListAsync(cancellationToken);
        return shifts.Select(Map).ToList();
    }

    private static CashShiftResponse Map(CashShift s) =>
        new(
            s.Id,
            s.WarehouseId,
            s.DeviceId,
            s.CashierUserId,
            s.Status.ToString(),
            s.OpeningFloat,
            s.ClosingCountedCash,
            s.OpenedAtUtc,
            s.ClosedAtUtc,
            s.RowVersion);
}
