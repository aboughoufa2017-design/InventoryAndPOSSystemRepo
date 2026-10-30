using Warehouse.Domain.Pos;

namespace Warehouse.UnitTests.Pos;

public sealed class PosSaleDomainTests
{
    [Fact]
    public void Completed_sale_can_void_once()
    {
        var utc = DateTimeOffset.UtcNow;
        var sale = PosSale.CreateCompleted(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "POS-1",
            "LYD",
            10m,
            0m,
            0m,
            10m,
            utc,
            null,
            null,
            null,
            null,
            null);

        Assert.Equal(PosSaleStatus.Completed, sale.Status);
        sale.Void(utc.AddMinutes(1));
        Assert.Equal(PosSaleStatus.Voided, sale.Status);
        Assert.Throws<InvalidOperationException>(() => sale.Void(utc.AddMinutes(2)));
    }

    [Fact]
    public void Line_rejects_non_positive_quantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PosSaleLine.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "SKU",
                "Name",
                0m,
                1m,
                0m,
                0m,
                0m));
    }
}
