using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyExchanger.Core.Models;
using CurrencyExchanger.Infrastructure.GatewayLibrary;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyExchanger.UnitTests;
public class CachedCurrencyGatewayTests
{
    [Fact]
    public async Task GetCurrencyRatesAsync_ShouldReturnCachedRates_WhenCacheIsValid()
    {
        // Arrange
        var cache = new CurrencyRateCache();
        var loggerMock = new Mock<ILogger<CachedCurrencyGateway>>();

        cache.UpdateRates(new List<CurrencyRate>
        {
            new CurrencyRate { CurrencyCode = "USD", Rate = 1.2m }
        });

        var gateway = new CachedCurrencyGateway(cache, loggerMock.Object);

        // Act
        var rates = await gateway.GetCurrencyRatesAsync();

        // Assert
        rates.Should().HaveCount(1);
        rates.First().CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task GetCurrencyRatesAsync_ShouldReturnEmptyList_WhenCacheIsEmpty()
    {
        // Arrange
        var cache = new CurrencyRateCache();
        var loggerMock = new Mock<ILogger<CachedCurrencyGateway>>();

        var gateway = new CachedCurrencyGateway(cache, loggerMock.Object);

        // Act
        var rates = await gateway.GetCurrencyRatesAsync();

        // Assert
        rates.Should().BeEmpty();
    }
}
