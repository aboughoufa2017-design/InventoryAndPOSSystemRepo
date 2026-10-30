using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Inventory;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Inventory;

public sealed class TransferService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : ITransferService
{
    public async Task<AppResult<TransferResponse>> CreateAsync(
        CreateTransferCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, "At least one line is required.");
        }

        if (command.SourceWarehouseId == command.DestinationWarehouseId)
        {
            return AppResult<TransferResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Source and destination warehouses must differ.");
        }

        var sourceWarehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.SourceWarehouseId, cancellationToken);
        var destinationWarehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.DestinationWarehouseId, cancellationToken);
        if (sourceWarehouse is null || destinationWarehouse is null)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var sourceLocations = await db.WarehouseLocations
            .Where(l => l.WarehouseId == sourceWarehouse.Id)
            .ToListAsync(cancellationToken);
        var destinationLocations = await db.WarehouseLocations
            .Where(l => l.WarehouseId == destinationWarehouse.Id)
            .ToListAsync(cancellationToken);

        var sourceLocation = ResolveLocation(sourceLocations, command.SourceLocationId, requireAvailable: true);
        var destinationLocation = ResolveLocation(destinationLocations, command.DestinationLocationId, requireAvailable: true);
        if (sourceLocation is null || destinationLocation is null)
        {
            return AppResult<TransferResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Source/destination location was not found or is not available for pick/put.");
        }

        var inTransit = sourceLocations.FirstOrDefault(l => l.Code == WarehouseLocationCodes.InTransit);
        if (inTransit is null)
        {
            return AppResult<TransferResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Source warehouse is missing an IN_TRANSIT location.");
        }

        var productIds = command.Lines.Select(l => l.ProductId).Distinct().ToList();
        if (productIds.Count != command.Lines.Count)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, "Duplicate products are not allowed.");
        }

        var products = await db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive && p.TrackInventory)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
        if (products.Count != productIds.Count)
        {
            return AppResult<TransferResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "One or more products were not found, inactive, or do not track inventory.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        Transfer transfer;
        List<TransferLine> lines;
        try
        {
            transfer = Transfer.Create(
                Guid.NewGuid(),
                tenantId,
                sourceWarehouse.Id,
                destinationWarehouse.Id,
                sourceLocation.Id,
                destinationLocation.Id,
                command.Notes,
                currentUser.UserId,
                utcNow);

            lines = command.Lines.Select(l => TransferLine.Create(
                Guid.NewGuid(),
                tenantId,
                transfer.Id,
                l.ProductId,
                l.Quantity,
                l.Notes)).ToList();
        }
        catch (ArgumentException ex)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        db.Transfers.Add(transfer);
        db.TransferLines.AddRange(lines);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<TransferResponse>.Success(Map(transfer, lines));
    }

    public async Task<AppResult<TransferResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var transfer = await db.Transfers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (transfer is null)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, "Transfer was not found.");
        }

        var lines = await db.TransferLines.AsNoTracking().Where(l => l.TransferId == id).ToListAsync(cancellationToken);
        return AppResult<TransferResponse>.Success(Map(transfer, lines));
    }

    public async Task<IReadOnlyList<TransferResponse>> ListAsync(string? status, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var query = db.Transfers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransferStatus>(status, true, out var parsed))
        {
            query = query.Where(t => t.Status == parsed);
        }

        var transfers = await query.OrderByDescending(t => t.CreatedAtUtc).Take(100).ToListAsync(cancellationToken);
        var ids = transfers.Select(t => t.Id).ToList();
        var lines = await db.TransferLines.AsNoTracking().Where(l => ids.Contains(l.TransferId)).ToListAsync(cancellationToken);
        return transfers.Select(t => Map(t, lines.Where(l => l.TransferId == t.Id).ToList())).ToList();
    }

    public async Task<AppResult<TransferResponse>> DispatchAsync(
        DispatchTransferCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var transfer = await db.Transfers.FirstOrDefaultAsync(t => t.Id == command.TransferId, cancellationToken);
        if (transfer is null)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, "Transfer was not found.");
        }

        try
        {
            transfer.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.SyncConflict, "Transfer was modified by another request.");
        }

        var lines = await db.TransferLines.Where(l => l.TransferId == transfer.Id).ToListAsync(cancellationToken);
        if (lines.Count == 0)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, "Transfer has no lines.");
        }

        var inTransit = await db.WarehouseLocations.FirstOrDefaultAsync(
            l => l.WarehouseId == transfer.SourceWarehouseId && l.Code == WarehouseLocationCodes.InTransit,
            cancellationToken);
        if (inTransit is null)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, "Source warehouse is missing IN_TRANSIT.");
        }

        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var allowNegative = currentUser.HasPermission("inventory.transfer");
        var utcNow = DateTimeOffset.UtcNow;

        try
        {
            foreach (var line in lines)
            {
                var decrease = await ApplyOnHandChangeAsync(
                    tenantId,
                    line.ProductId,
                    transfer.SourceWarehouseId,
                    transfer.SourceLocationId,
                    -line.QuantityRequested,
                    tenant.NegativeStockPolicy,
                    allowNegative,
                    utcNow,
                    cancellationToken);
                if (!decrease.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<TransferResponse>.Failure(decrease.ErrorCode!, decrease.ErrorMessage!);
                }

                var increase = await ApplyOnHandChangeAsync(
                    tenantId,
                    line.ProductId,
                    transfer.SourceWarehouseId,
                    inTransit.Id,
                    line.QuantityRequested,
                    NegativeStockPolicy.Allow,
                    true,
                    utcNow,
                    cancellationToken);
                if (!increase.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<TransferResponse>.Failure(increase.ErrorCode!, increase.ErrorMessage!);
                }

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, transfer.SourceWarehouseId, transfer.SourceLocationId,
                    -line.QuantityRequested, StockMovementType.TransferDispatch, SourceDocumentTypes.Transfer,
                    transfer.Id, line.Id, currentUser.UserId, "Transfer dispatch (source)", utcNow));

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, transfer.SourceWarehouseId, inTransit.Id,
                    line.QuantityRequested, StockMovementType.TransferDispatch, SourceDocumentTypes.Transfer,
                    transfer.Id, line.Id, currentUser.UserId, "Transfer dispatch (in-transit)", utcNow));
            }

            transfer.MarkDispatched(command.OperationId, utcNow);
            var response = Map(transfer, lines);
            if (command.OperationId is Guid operationId)
            {
                db.TransferOperations.Add(TransferOperation.Create(
                    operationId,
                    tenantId,
                    transfer.Id,
                    TransferOperationActions.Dispatch,
                    JsonSerializer.Serialize(response, TransferJson.Options),
                    utcNow));
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<TransferResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            await tx.RollbackAsync(cancellationToken);
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<TransferResponse>> ReceiveAsync(
        ReceiveTransferCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, "At least one receive line is required.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var transfer = await db.Transfers.FirstOrDefaultAsync(t => t.Id == command.TransferId, cancellationToken);
        if (transfer is null)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, "Transfer was not found.");
        }

        try
        {
            transfer.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.SyncConflict, "Transfer was modified by another request.");
        }

        var lines = await db.TransferLines.Where(l => l.TransferId == transfer.Id).ToListAsync(cancellationToken);
        var inTransit = await db.WarehouseLocations.FirstAsync(
            l => l.WarehouseId == transfer.SourceWarehouseId && l.Code == WarehouseLocationCodes.InTransit,
            cancellationToken);
        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var allowNegative = currentUser.HasPermission("inventory.transfer");
        var utcNow = DateTimeOffset.UtcNow;

        try
        {
            foreach (var receiveLine in command.Lines)
            {
                var line = lines.FirstOrDefault(l => l.Id == receiveLine.TransferLineId);
                if (line is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, "Transfer line was not found.");
                }

                line.Receive(receiveLine.Quantity);

                var leaveTransit = await ApplyOnHandChangeAsync(
                    tenantId, line.ProductId, transfer.SourceWarehouseId, inTransit.Id,
                    -receiveLine.Quantity, tenant.NegativeStockPolicy, allowNegative, utcNow, cancellationToken);
                if (!leaveTransit.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<TransferResponse>.Failure(leaveTransit.ErrorCode!, leaveTransit.ErrorMessage!);
                }

                var arriveDest = await ApplyOnHandChangeAsync(
                    tenantId, line.ProductId, transfer.DestinationWarehouseId, transfer.DestinationLocationId,
                    receiveLine.Quantity, NegativeStockPolicy.Allow, true, utcNow, cancellationToken);
                if (!arriveDest.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<TransferResponse>.Failure(arriveDest.ErrorCode!, arriveDest.ErrorMessage!);
                }

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, transfer.SourceWarehouseId, inTransit.Id,
                    -receiveLine.Quantity, StockMovementType.TransferReceive, SourceDocumentTypes.Transfer,
                    transfer.Id, line.Id, currentUser.UserId, "Transfer receive (leave in-transit)", utcNow));

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, transfer.DestinationWarehouseId, transfer.DestinationLocationId,
                    receiveLine.Quantity, StockMovementType.TransferReceive, SourceDocumentTypes.Transfer,
                    transfer.Id, line.Id, currentUser.UserId, "Transfer receive (destination)", utcNow));
            }

            transfer.ApplyReceiveProgress(lines.Any(l => l.QuantityInTransit > 0m), utcNow);
            var response = Map(transfer, lines);
            if (command.OperationId is Guid operationId)
            {
                db.TransferOperations.Add(TransferOperation.Create(
                    operationId,
                    tenantId,
                    transfer.Id,
                    TransferOperationActions.Receive,
                    JsonSerializer.Serialize(response, TransferJson.Options),
                    utcNow));
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<TransferResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<TransferResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            await tx.RollbackAsync(cancellationToken);
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<TransferResponse>> CancelAsync(
        CancelTransferCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var transfer = await db.Transfers.FirstOrDefaultAsync(t => t.Id == command.TransferId, cancellationToken);
        if (transfer is null)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, "Transfer was not found.");
        }

        try
        {
            transfer.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.SyncConflict, "Transfer was modified by another request.");
        }

        var lines = await db.TransferLines.Where(l => l.TransferId == transfer.Id).ToListAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;

        try
        {
            if (transfer.Status == TransferStatus.Draft)
            {
                transfer.CancelDraft(utcNow);
            }
            else if (transfer.Status is TransferStatus.Dispatched or TransferStatus.PartiallyReceived)
            {
                var inTransit = await db.WarehouseLocations.FirstAsync(
                    l => l.WarehouseId == transfer.SourceWarehouseId && l.Code == WarehouseLocationCodes.InTransit,
                    cancellationToken);
                var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
                var allowNegative = currentUser.HasPermission("inventory.transfer");

                foreach (var line in lines.Where(l => l.QuantityInTransit > 0m))
                {
                    var qty = line.QuantityInTransit;
                    var leaveTransit = await ApplyOnHandChangeAsync(
                        tenantId, line.ProductId, transfer.SourceWarehouseId, inTransit.Id,
                        -qty, tenant.NegativeStockPolicy, allowNegative, utcNow, cancellationToken);
                    if (!leaveTransit.IsSuccess)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        return AppResult<TransferResponse>.Failure(leaveTransit.ErrorCode!, leaveTransit.ErrorMessage!);
                    }

                    var returnSource = await ApplyOnHandChangeAsync(
                        tenantId, line.ProductId, transfer.SourceWarehouseId, transfer.SourceLocationId,
                        qty, NegativeStockPolicy.Allow, true, utcNow, cancellationToken);
                    if (!returnSource.IsSuccess)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        return AppResult<TransferResponse>.Failure(returnSource.ErrorCode!, returnSource.ErrorMessage!);
                    }

                    db.StockMovements.Add(StockMovement.Create(
                        Guid.NewGuid(), tenantId, line.ProductId, transfer.SourceWarehouseId, inTransit.Id,
                        -qty, StockMovementType.TransferReceive, SourceDocumentTypes.Transfer,
                        transfer.Id, line.Id, currentUser.UserId, "Transfer cancel (leave in-transit)", utcNow));
                    db.StockMovements.Add(StockMovement.Create(
                        Guid.NewGuid(), tenantId, line.ProductId, transfer.SourceWarehouseId, transfer.SourceLocationId,
                        qty, StockMovementType.TransferDispatch, SourceDocumentTypes.Transfer,
                        transfer.Id, line.Id, currentUser.UserId, "Transfer cancel (return source)", utcNow));

                    line.ReturnAllInTransitToSource();
                }

                transfer.CancelInTransit(utcNow);
            }
            else
            {
                return AppResult<TransferResponse>.Failure(
                    ErrorCodes.InvalidOperation,
                    $"Transfer is {transfer.Status} and cannot be cancelled.");
            }

            var response = Map(transfer, lines);
            if (command.OperationId is Guid operationId)
            {
                db.TransferOperations.Add(TransferOperation.Create(
                    operationId,
                    tenantId,
                    transfer.Id,
                    TransferOperationActions.Cancel,
                    JsonSerializer.Serialize(response, TransferJson.Options),
                    utcNow));
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<TransferResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<TransferResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    private async Task<AppResult<TransferResponse>?> TryReplayAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var existing = await db.TransferOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operationId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var payload = JsonSerializer.Deserialize<TransferResponse>(existing.ResultPayloadJson, TransferJson.Options);
        if (payload is null)
        {
            return AppResult<TransferResponse>.Failure(ErrorCodes.InternalError, "Idempotent payload could not be read.");
        }

        return AppResult<TransferResponse>.Success(payload with { WasIdempotentReplay = true });
    }

    private async Task<AppResult> ApplyOnHandChangeAsync(
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        decimal quantityChange,
        NegativeStockPolicy policy,
        bool allowNegativePermission,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var balance = await db.StockBalances.FirstOrDefaultAsync(
            b => b.ProductId == productId && b.WarehouseId == warehouseId && b.LocationId == locationId,
            cancellationToken);

        if (balance is null)
        {
            balance = StockBalance.CreateEmpty(Guid.NewGuid(), tenantId, productId, warehouseId, locationId, utcNow);
            db.StockBalances.Add(balance);
        }

        var resulting = balance.OnHand + quantityChange;
        if (!NegativeStockEvaluator.IsAllowed(policy, resulting, allowNegativePermission))
        {
            return AppResult.Failure(ErrorCodes.InsufficientStock, "Insufficient stock for transfer operation.");
        }

        balance.ApplyOnHandDelta(quantityChange, utcNow);
        return AppResult.Success();
    }

    private static WarehouseLocation? ResolveLocation(
        IReadOnlyList<WarehouseLocation> locations,
        Guid? requestedId,
        bool requireAvailable)
    {
        WarehouseLocation? location;
        if (requestedId is Guid id)
        {
            location = locations.FirstOrDefault(l => l.Id == id);
        }
        else
        {
            location = locations.FirstOrDefault(l => l.Code == WarehouseLocationCodes.General)
                       ?? locations.FirstOrDefault(l => l.IsDefault);
        }

        if (location is null)
        {
            return null;
        }

        if (requireAvailable && !location.IsAvailableForSaleOrPick)
        {
            return null;
        }

        return location;
    }

    private static TransferResponse Map(Transfer transfer, IReadOnlyList<TransferLine> lines) =>
        new(
            transfer.Id,
            transfer.SourceWarehouseId,
            transfer.DestinationWarehouseId,
            transfer.SourceLocationId,
            transfer.DestinationLocationId,
            transfer.Status.ToString(),
            transfer.Notes,
            transfer.CreatedAtUtc,
            transfer.DispatchedAtUtc,
            transfer.CompletedAtUtc,
            transfer.RowVersion,
            lines.Select(l => new TransferLineResponse(
                l.Id, l.ProductId, l.QuantityRequested, l.QuantityReceived, l.QuantityInTransit, l.Notes)).ToList());

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}
