using System.Collections.Generic;
using System.Threading.Tasks;
using CurrencyExchanger.Core.Models;

namespace CurrencyExchanger.Infrastructure.GatewayLibrary
{
    public interface ICurrencyGateway
    {
        Task<string> GetCurrencyRatesRawXmlAsync();
        Task<IEnumerable<CurrencyRate>> GetCurrencyRatesAsync();
    }
}
