using CurrencyExchanger.Core.Models;
using CurrencyExchanger.Infrastructure.GatewayLibrary;
using FluentAssertions;
using Shouldly;

namespace CurrencyExchanger.UnitTests;

public class CurrencyRateCacheTests
{
    [Fact]
    public void UpdateRates_ShouldUpdateLatestRates()
    {
        // Arrange
        var cache = new CurrencyRateCache();
        var rates = new List<CurrencyRate>
        {
            new() { CurrencyCode = "USD", Rate = 1.2m },
            new() { CurrencyCode = "CAD", Rate = 1.0m }
        };

        // Act
        cache.UpdateRates(rates);

        // Assert
        cache.LatestRates.Should().HaveCount(2);
        cache.LatestRates.Should().Contain(rate => rate.CurrencyCode == "USD");
        cache.LatestRates.Should().Contain(rate => rate.CurrencyCode == "CAD");
        cache.LatestRates.ShouldNotContain(rate => rate.CurrencyCode == "EUR");
    }

    [Fact]
    public void UpdateRates_ShouldHandleEmptyRateList()
    {
        // Arrange
        var cache = new CurrencyRateCache();

        // Act
        cache.UpdateRates(new List<CurrencyRate>());

        // Assert
        cache.LatestRates.Should().BeEmpty();
    }
}