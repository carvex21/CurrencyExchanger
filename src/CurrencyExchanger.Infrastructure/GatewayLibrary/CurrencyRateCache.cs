using CurrencyExchanger.Core.Models;

namespace CurrencyExchanger.Infrastructure.GatewayLibrary
{
    public class CurrencyRateCache
    {
        public IEnumerable<CurrencyRate> LatestRates { get; private set; } = Enumerable.Empty<CurrencyRate>();

        public void UpdateRates(IEnumerable<CurrencyRate> rates)
        {
            LatestRates = rates;
        }
    }
}