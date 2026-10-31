using Warehouse.Application.Messaging;
using Xunit;

namespace Warehouse.UnitTests.Messaging;

public sealed class ResalaPhoneTests
{
    [Theory]
    [InlineData("0910001234", true)]
    [InlineData("0920001234", true)]
    [InlineData("0930001234", true)]
    [InlineData("0940001234", true)]
    [InlineData("218910001234", true)]
    [InlineData("+218 91-000-1234", true)]
    [InlineData("091000123", false)]
    [InlineData("0900001234", false)]
    [InlineData("15551234567", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsLibyanMobile_matches_carrier_rules(string? phone, bool expected)
    {
        Assert.Equal(expected, ResalaPhone.IsLibyanMobile(phone));
    }

    [Theory]
    [InlineData("0910001234", "218910001234")]
    [InlineData("218910001234", "218910001234")]
    [InlineData("+218920001234", "218920001234")]
    [InlineData("15551234567", null)]
    public void NormalizeLibyanMobile_to_218(string? phone, string? expected)
    {
        Assert.Equal(expected, ResalaPhone.NormalizeLibyanMobile(phone));
    }
}
