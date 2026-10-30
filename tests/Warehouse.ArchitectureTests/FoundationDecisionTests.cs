using System.Reflection;
using Warehouse.Domain.Commercial;
using Warehouse.Domain.Common;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Precision;
using Warehouse.Domain.Security;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;

namespace Warehouse.ArchitectureTests;

public class FoundationDecisionTests
{
    [Fact]
    public void Money_uses_numeric_19_4()
    {
        Assert.Equal(19, MoneyPrecision.Precision);
        Assert.Equal(4, MoneyPrecision.Scale);
    }

    [Fact]
    public void Quantity_uses_numeric_18_6()
    {
        Assert.Equal(18, QuantityPrecision.Precision);
        Assert.Equal(6, QuantityPrecision.Scale);
    }

    [Fact]
    public void Commercial_payment_status_has_unpaid_partially_paid_and_paid_only()
    {
        Assert.Equal(["Unpaid", "PartiallyPaid", "Paid"], Enum.GetNames<PaymentStatus>());
    }

    [Fact]
    public void Negative_stock_policies_exclude_warn()
    {
        var names = Enum.GetNames<NegativeStockPolicy>();
        Assert.Equal(["Prevent", "Allow", "AllowWithPermission"], names);
        Assert.DoesNotContain("Warn", names);
    }

    [Fact]
    public void Tax_is_disabled_by_default()
    {
        Assert.False(TaxPolicy.CreateDefault().Enabled);
    }

    [Fact]
    public void Transfer_dispatch_moves_available_to_in_transit_semantics()
    {
        Assert.Equal(StockBalanceKind.Available, TransferStockEffects.DispatchSourceKind);
        Assert.Equal(StockBalanceKind.InTransit, TransferStockEffects.DispatchDestinationKind);
        Assert.Contains("LocationId(NOT NULL)", TransferStockEffects.BalanceGrainDescription, StringComparison.Ordinal);
        Assert.Contains("OnHand", TransferStockEffects.BalanceGrainDescription, StringComparison.Ordinal);
        Assert.Contains("Reserved", TransferStockEffects.BalanceGrainDescription, StringComparison.Ordinal);
        Assert.Equal(WarehouseLocationCodes.InTransit, WarehouseLocationCodes.InTransit);
        Assert.Equal(WarehouseLocationCodes.General, WarehouseLocationCodes.General);
    }

    [Fact]
    public void Transfer_receive_moves_in_transit_to_available_semantics()
    {
        Assert.Equal(StockBalanceKind.InTransit, TransferStockEffects.ReceiveSourceKind);
        Assert.Equal(StockBalanceKind.Available, TransferStockEffects.ReceiveDestinationKind);
    }

    [Fact]
    public void Offline_default_policy_uses_seven_day_max_offline_days()
    {
        var policy = OfflineSessionPolicy.CreateDefault();

        Assert.Equal(OfflineSessionPolicy.DefaultMaxOfflineDays, policy.MaxOfflineDays);
        Assert.Equal(7, OfflineSessionPolicy.DefaultMaxOfflineDays);
        Assert.False(policy.RequireOnlineRevalidation);
        Assert.False(policy.DeviceRevoked);
        Assert.False(policy.UserDisabled);
        Assert.False(policy.TenantSuspended);
    }

    [Fact]
    public void Device_security_policy_exposes_revocation_user_and_tenant_blocks()
    {
        var policy = new OfflineSessionPolicy(
            maxOfflineDays: OfflineSessionPolicy.DefaultMaxOfflineDays,
            lastSuccessfulOnlineAuthentication: DateTimeOffset.UtcNow,
            requireOnlineRevalidation: false,
            deviceRevoked: true,
            userDisabled: false,
            tenantSuspended: false);

        var devicePolicy = new DeviceSecurityPolicy(policy);

        Assert.True(devicePolicy.DeviceRevoked);
        Assert.True(policy.BlocksServerApply);
        Assert.True(policy.MustStopCreatingLocalOperations(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Tenant_owned_entities_expose_tenant_id()
    {
        var property = typeof(TenantOwnedEntity).GetProperty(
            nameof(TenantOwnedEntity.TenantId),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(property);
        Assert.Equal(typeof(Guid), property!.PropertyType);
    }

    [Fact]
    public void Document_lifecycle_status_is_draft_approved_posted_cancelled()
    {
        Assert.Equal(
            ["Draft", "Approved", "Posted", "Cancelled"],
            Enum.GetNames<DocumentLifecycleStatus>());
    }
}
