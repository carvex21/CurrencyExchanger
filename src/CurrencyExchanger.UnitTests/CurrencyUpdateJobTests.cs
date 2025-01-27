using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyExchanger.Api.Models;
using CurrencyExchanger.Api.Workers;
using CurrencyExchanger.Core.Models;
using CurrencyExchanger.Infrastructure;
using CurrencyExchanger.Infrastructure.GatewayLibrary;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CurrencyExchanger.UnitTests;

public class CurrencyUpdateJobTests
{
    /*[Fact]
    public async Task ExecuteAsync_ShouldUpdateCacheAndDatabase_WhenRatesAreFetched()
    {
        // Arrange
        var cache = new CurrencyRateCache();
        var loggerMock = new Mock<ILogger<CurrencyUpdateJob>>();
        var gatewayMock = new Mock<CurrencyGateway>();
        var dbContextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

        gatewayMock.Setup(g => g.GetCurrencyRatesAsync())
            .ReturnsAsync(new List<CurrencyRate>
            {
                new CurrencyRate { CurrencyCode = "USD", Rate = 1.2m }
            });

        dbContextMock.Setup(db => db.Database.ExecuteSqlRawAsync(It.IsAny<string>(), default))
            .Returns(Task.FromResult(0));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(CurrencyGateway))).Returns(gatewayMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(AppDbContext))).Returns(dbContextMock.Object);

        var optionsMock = new Mock<IOptions<EcbWorker>>();
        optionsMock.Setup(o => o.Value).Returns(new EcbWorker { RefreshFrequency = TimeSpan.FromMinutes(1) });

        var job = new CurrencyUpdateJob(serviceProviderMock.Object, cache, loggerMock.Object, optionsMock.Object);

        // Act
        await job.StartAsync(default);

        // Assert
        cache.LatestRates.Should().HaveCount(1);
        cache.LatestRates.First().CurrencyCode.Should().Be("USD");
    }*/
}
