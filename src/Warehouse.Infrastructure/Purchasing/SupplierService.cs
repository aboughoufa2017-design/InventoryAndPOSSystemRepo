using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Purchasing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Purchasing;

public sealed class SupplierService(
    WarehouseDbContext db,
    ITenantContext tenantContext) : ISupplierService
{
    public async Task<AppResult<SupplierResponse>> CreateAsync(
        CreateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SupplierResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        Supplier supplier;
        try
        {
            supplier = Supplier.Create(
                Guid.NewGuid(),
                tenantId,
                command.Code,
                command.Name,
                command.ContactName,
                command.Email,
                command.Phone,
                command.Notes,
                DateTimeOffset.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return AppResult<SupplierResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        var exists = await db.Suppliers.AnyAsync(
            s => s.Code == supplier.Code,
            cancellationToken);
        if (exists)
        {
            return AppResult<SupplierResponse>.Failure(ErrorCodes.ValidationFailed, "Supplier code already exists.");
        }

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SupplierResponse>.Success(Map(supplier));
    }

    public async Task<AppResult<SupplierResponse>> UpdateAsync(
        UpdateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == command.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return AppResult<SupplierResponse>.Failure(ErrorCodes.InvalidOperation, "Supplier was not found.");
        }

        try
        {
            supplier.EnsureRowVersion(command.ExpectedRowVersion);
            supplier.Update(
                command.Name,
                command.ContactName,
                command.Email,
                command.Phone,
                command.Notes,
                command.IsActive,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex) when (ex.Message == "SYNC_CONFLICT")
        {
            return AppResult<SupplierResponse>.Failure(ErrorCodes.SyncConflict, "Supplier was modified by another request.");
        }
        catch (ArgumentException ex)
        {
            return AppResult<SupplierResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SupplierResponse>.Success(Map(supplier));
    }

    public async Task<AppResult<SupplierResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var supplier = await db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (supplier is null)
        {
            return AppResult<SupplierResponse>.Failure(ErrorCodes.InvalidOperation, "Supplier was not found.");
        }

        return AppResult<SupplierResponse>.Success(Map(supplier));
    }

    public async Task<IReadOnlyList<SupplierResponse>> ListAsync(CancellationToken cancellationToken)
    {
        EnsureTenant();
        var items = await db.Suppliers.AsNoTracking()
            .OrderBy(s => s.Code)
            .Take(200)
            .ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    private static SupplierResponse Map(Supplier s) =>
        new(s.Id, s.Code, s.Name, s.ContactName, s.Email, s.Phone, s.Notes, s.IsActive, s.CreatedAtUtc, s.RowVersion);

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}
