using CurrencyExchanger.Core.Models;
using Microsoft.Extensions.Logging;

namespace CurrencyExchanger.Infrastructure.GatewayLibrary
{
    public class CachedCurrencyGateway : ICurrencyGateway
    {
        private readonly CurrencyRateCache _cache;
        private readonly ILogger<CachedCurrencyGateway> _logger;

        public CachedCurrencyGateway(CurrencyRateCache cache, ILogger<CachedCurrencyGateway> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<IEnumerable<CurrencyRate>> GetCurrencyRatesAsync()
        {
            if (!_cache.LatestRates.Any())
            {
                _logger.LogWarning(">>Cache is empty. Returning no rates<<");
                return Task.FromResult(Enumerable.Empty<CurrencyRate>());
            }

            _logger.LogInformation("++Returning cached currency rates++");
            return Task.FromResult(_cache.LatestRates);
        }

        public Task<string> GetCurrencyRatesRawXmlAsync()
        {
            _logger.LogWarning(">>Raw XML caching is not implemented<<");
            throw new NotImplementedException();
        }
    }
}