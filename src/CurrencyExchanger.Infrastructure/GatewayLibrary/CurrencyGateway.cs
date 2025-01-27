using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using CurrencyExchanger.Core.Models;

namespace CurrencyExchanger.Infrastructure.GatewayLibrary
{
    public class CurrencyGateway : ICurrencyGateway
    {
        private readonly HttpClient _httpClient;

        private const string EcbEndpoint = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

        public CurrencyGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetCurrencyRatesRawXmlAsync()
        {
            var response = await _httpClient.GetAsync(EcbEndpoint);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<IEnumerable<CurrencyRate>> GetCurrencyRatesAsync()
        {
            var rawXml = await GetCurrencyRatesRawXmlAsync();
            var currencyRates = ParseCurrencyRates(rawXml);
            return currencyRates;
        }

        private IEnumerable<CurrencyRate> ParseCurrencyRates(string xml)
        {
            var document = XDocument.Parse(xml);
            var namespaces = document.Root.GetDefaultNamespace();

            var requestDate = document.Descendants(namespaces + "Cube")
                .FirstOrDefault(e => e.Attribute("time") != null);
            var dateParsed = DateTime.Parse(requestDate.Attribute("time").Value);
            
            var rates = new List<CurrencyRate>();

            foreach (var element in document
                         .Descendants(namespaces + "Cube")
                         .Where(e => e.Attribute("currency") != null))
            {
                var currency = element.Attribute("currency").Value;
                var rate = decimal.Parse(element.Attribute("rate").Value);

                rates.Add(new CurrencyRate
                {
                    CurrencyCode = currency,
                    Rate = rate,
                    Date = dateParsed
                });
            }

            return rates;
        }
    }
}
