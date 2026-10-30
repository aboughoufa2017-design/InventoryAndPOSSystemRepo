using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Sales;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Sales;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Sales;

public sealed class CustomerService(
    WarehouseDbContext db,
    ITenantContext tenantContext) : ICustomerService
{
    public async Task<AppResult<CustomerResponse>> CreateAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<CustomerResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        Customer customer;
        try
        {
            customer = Customer.Create(
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
            return AppResult<CustomerResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        if (await db.Customers.AnyAsync(c => c.Code == customer.Code, cancellationToken))
        {
            return AppResult<CustomerResponse>.Failure(ErrorCodes.ValidationFailed, "Customer code already exists.");
        }

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<CustomerResponse>.Success(Map(customer));
    }

    public async Task<AppResult<CustomerResponse>> UpdateAsync(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == command.CustomerId, cancellationToken);
        if (customer is null)
        {
            return AppResult<CustomerResponse>.Failure(ErrorCodes.InvalidOperation, "Customer was not found.");
        }

        try
        {
            customer.EnsureRowVersion(command.ExpectedRowVersion);
            customer.Update(
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
            return AppResult<CustomerResponse>.Failure(ErrorCodes.SyncConflict, "Customer was modified by another request.");
        }
        catch (ArgumentException ex)
        {
            return AppResult<CustomerResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<CustomerResponse>.Success(Map(customer));
    }

    public async Task<AppResult<CustomerResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var customer = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer is null)
        {
            return AppResult<CustomerResponse>.Failure(ErrorCodes.InvalidOperation, "Customer was not found.");
        }

        return AppResult<CustomerResponse>.Success(Map(customer));
    }

    public async Task<IReadOnlyList<CustomerResponse>> ListAsync(CancellationToken cancellationToken)
    {
        EnsureTenant();
        var items = await db.Customers.AsNoTracking()
            .OrderBy(c => c.Code)
            .Take(200)
            .ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    private static CustomerResponse Map(Customer c) =>
        new(c.Id, c.Code, c.Name, c.ContactName, c.Email, c.Phone, c.Notes, c.IsActive, c.CreatedAtUtc, c.RowVersion);

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}
