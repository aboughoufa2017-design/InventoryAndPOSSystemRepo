using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Pos;

public enum CashShiftStatus
{
    Open = 0,
    Closed = 1,
}

public enum CashMovementType
{
    PayIn = 0,
    PayOut = 1,
}

public sealed class CashShift : TenantOwnedEntity
{
    private CashShift()
    {
    }

    private CashShift(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public Guid? DeviceId { get; private set; }

    public Guid CashierUserId { get; private set; }

    public CashShiftStatus Status { get; private set; } = CashShiftStatus.Open;

    public decimal OpeningFloat { get; private set; }

    public decimal? ClosingCountedCash { get; private set; }

    public DateTimeOffset OpenedAtUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static CashShift Open(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        Guid cashierUserId,
        decimal openingFloat,
        DateTimeOffset utcNow,
        Guid? deviceId)
    {
        if (warehouseId == Guid.Empty || cashierUserId == Guid.Empty)
        {
            throw new ArgumentException("Warehouse and cashier are required.");
        }

        return new CashShift(id, tenantId)
        {
            WarehouseId = warehouseId,
            DeviceId = deviceId,
            CashierUserId = cashierUserId,
            OpeningFloat = Math.Round(openingFloat, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
            OpenedAtUtc = utcNow,
        };
    }

    public void Close(decimal countedCash, DateTimeOffset utcNow)
    {
        if (Status != CashShiftStatus.Open)
        {
            throw new InvalidOperationException("Shift is already closed.");
        }

        Status = CashShiftStatus.Closed;
        ClosingCountedCash = Math.Round(countedCash, MoneyPrecision.Scale, MidpointRounding.AwayFromZero);
        ClosedAtUtc = utcNow;
        RowVersion++;
    }
}

public sealed class CashMovement : TenantOwnedEntity
{
    private CashMovement()
    {
    }

    private CashMovement(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid CashShiftId { get; private set; }

    public CashMovementType Type { get; private set; }

    public decimal Amount { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static CashMovement Create(
        Guid id,
        Guid tenantId,
        Guid cashShiftId,
        CashMovementType type,
        decimal amount,
        string? reason,
        DateTimeOffset utcNow)
    {
        if (cashShiftId == Guid.Empty || amount <= 0m)
        {
            throw new ArgumentException("Shift and positive amount are required.");
        }

        return new CashMovement(id, tenantId)
        {
            CashShiftId = cashShiftId,
            Type = type,
            Amount = Math.Round(amount, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedAtUtc = utcNow,
        };
    }
}

public enum PosConflictStatus
{
    Open = 0,
    Resolved = 1,
}

/// <summary>Flagged when offline POS sales drive stock negative or compete.</summary>
public sealed class PosInventoryConflict : TenantOwnedEntity
{
    private PosInventoryConflict()
    {
    }

    private PosInventoryConflict(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public Guid ProductId { get; private set; }

    public Guid? SourceCommandId { get; private set; }

    public Guid? PosSaleId { get; private set; }

    public decimal ResultingOnHand { get; private set; }

    public string Message { get; private set; } = string.Empty;

    public PosConflictStatus Status { get; private set; } = PosConflictStatus.Open;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ResolvedAtUtc { get; private set; }

    public static PosInventoryConflict Open(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        Guid productId,
        decimal resultingOnHand,
        string message,
        DateTimeOffset utcNow,
        Guid? sourceCommandId,
        Guid? posSaleId)
    {
        return new PosInventoryConflict(id, tenantId)
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            ResultingOnHand = Math.Round(resultingOnHand, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
            Message = message.Trim(),
            SourceCommandId = sourceCommandId,
            PosSaleId = posSaleId,
            CreatedAtUtc = utcNow,
        };
    }

    public void Resolve(DateTimeOffset utcNow)
    {
        Status = PosConflictStatus.Resolved;
        ResolvedAtUtc = utcNow;
    }
}

public sealed class PriceList : TenantOwnedEntity
{
    private PriceList()
    {
    }

    private PriceList(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string CurrencyCode { get; private set; } = "LYD";

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static PriceList Create(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string currencyCode,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new PriceList(id, tenantId)
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "LYD" : currencyCode.Trim().ToUpperInvariant(),
            CreatedAtUtc = utcNow,
        };
    }
}

public sealed class ProductPrice : TenantOwnedEntity
{
    private ProductPrice()
    {
    }

    private ProductPrice(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PriceListId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal? MinQuantity { get; private set; }

    public static ProductPrice Create(
        Guid id,
        Guid tenantId,
        Guid priceListId,
        Guid productId,
        decimal unitPrice,
        decimal? minQuantity)
    {
        return new ProductPrice(id, tenantId)
        {
            PriceListId = priceListId,
            ProductId = productId,
            UnitPrice = Math.Round(unitPrice, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
            MinQuantity = minQuantity is null
                ? null
                : Math.Round(minQuantity.Value, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
        };
    }
}

/// <summary>Minimal restaurant open ticket for pilot profiles.</summary>
public sealed class RestaurantTable : TenantOwnedEntity
{
    private RestaurantTable()
    {
    }

    private RestaurantTable(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public static RestaurantTable Create(Guid id, Guid tenantId, Guid warehouseId, string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new RestaurantTable(id, tenantId)
        {
            WarehouseId = warehouseId,
            Code = code.Trim().ToUpperInvariant(),
            Name = string.IsNullOrWhiteSpace(name) ? code.Trim() : name.Trim(),
        };
    }
}

public enum RestaurantOrderStatus
{
    Open = 0,
    Closed = 1,
    Cancelled = 2,
}

public sealed class RestaurantOpenOrder : TenantOwnedEntity
{
    private RestaurantOpenOrder()
    {
    }

    private RestaurantOpenOrder(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public Guid TableId { get; private set; }

    public RestaurantOrderStatus Status { get; private set; } = RestaurantOrderStatus.Open;

    public string? Notes { get; private set; }

    public DateTimeOffset OpenedAtUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public Guid? PosSaleId { get; private set; }

    public static RestaurantOpenOrder Open(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        Guid tableId,
        string? notes,
        DateTimeOffset utcNow)
    {
        return new RestaurantOpenOrder(id, tenantId)
        {
            WarehouseId = warehouseId,
            TableId = tableId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            OpenedAtUtc = utcNow,
        };
    }

    public void CloseWithSale(Guid posSaleId, DateTimeOffset utcNow)
    {
        if (Status != RestaurantOrderStatus.Open)
        {
            throw new InvalidOperationException("Order is not open.");
        }

        Status = RestaurantOrderStatus.Closed;
        PosSaleId = posSaleId;
        ClosedAtUtc = utcNow;
    }

    public void Cancel(DateTimeOffset utcNow)
    {
        if (Status != RestaurantOrderStatus.Open)
        {
            throw new InvalidOperationException("Order is not open.");
        }

        Status = RestaurantOrderStatus.Cancelled;
        ClosedAtUtc = utcNow;
    }
}
