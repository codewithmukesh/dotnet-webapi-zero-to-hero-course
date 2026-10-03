using Api.Orders;
using NSubstitute;

namespace Api.Tests;

public class OrderCalculatorTests
{
    [Fact]
    public async Task Total_multiplies_price_by_quantity()
    {
        var ct = TestContext.Current.CancellationToken;

        var prices = Substitute.For<IPriceService>();
        prices.GetPriceAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(25m);

        var total = await new OrderCalculator(prices).TotalAsync(Guid.NewGuid(), 4, ct);

        Assert.Equal(100m, total);
    }
}
