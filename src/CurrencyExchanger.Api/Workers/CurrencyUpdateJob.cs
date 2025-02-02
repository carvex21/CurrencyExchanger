using System.Text;
using CurrencyExchanger.Api.Models;
using CurrencyExchanger.Core.Models;
using CurrencyExchanger.Infrastructure;
using CurrencyExchanger.Infrastructure.GatewayLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CurrencyExchanger.Api.Workers
{
    public class CurrencyUpdateJob : BackgroundService
    {
        private readonly ICurrencyGateway _gateway;
        private readonly CurrencyRateCache _cache;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<CurrencyUpdateJob> _logger;
        private readonly TimeSpan _refreshFrequency;

        public CurrencyUpdateJob(CurrencyRateCache cache,
            ILogger<CurrencyUpdateJob> logger, IOptions<EcbWorker> options, AppDbContext dbContext, ICurrencyGateway gateway)
        {
            _cache = cache;
            _logger = logger;
            _dbContext = dbContext;
            _gateway = gateway;
            _refreshFrequency = options.Value.RefreshFrequency;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("~~CurrencyUpdateJob is starting~~");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("~~Fetching fresh currency rates~~");

                    var rates = await _gateway.GetCurrencyRatesAsync();

                    if (rates.Any())
                    {
                        _cache.UpdateRates(rates);
                        _logger.LogInformation("++Cache updated with {Count} rates++", rates.Count());

                        await UpdateDatabaseAsync(rates);
                    }
                    else
                    {
                        _logger.LogWarning(">>No currency rates were fetched<<");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ">>Error during currency rate update<<");
                }

                _logger.LogInformation($"~~Waiting {_refreshFrequency.TotalMinutes} minutes before the next update~~");
                await Task.Delay(_refreshFrequency, stoppingToken);
            }

            _logger.LogInformation("~~CurrencyUpdateJob is stopping~~");
        }

        private async Task UpdateDatabaseAsync(IEnumerable<CurrencyRate> rates)
        {
            _logger.LogInformation("~~Updating database with fresh currency rates~~");

            var mergeCommand = BuildMergeCommand(rates);
            await _dbContext.Database.ExecuteSqlRawAsync(mergeCommand);

            _logger.LogInformation("++Database updated successfully++");
        }

        private string BuildMergeCommand(IEnumerable<CurrencyRate> rates)
        {
            var sb = new StringBuilder();

            sb.AppendLine("MERGE INTO CurrencyRates AS Target");
            sb.AppendLine("USING (VALUES");

            var valueTuples = rates.Select(r => $"('{r.CurrencyCode}', {r.Rate}, '{r.Date:yyyy-MM-dd}')");
            sb.AppendLine(string.Join(",\n", valueTuples));

            sb.AppendLine(") AS Source (CurrencyCode, Rate, Date)");
            sb.AppendLine("ON Target.CurrencyCode = Source.CurrencyCode AND Target.Date = Source.Date");
            sb.AppendLine("WHEN MATCHED THEN");
            sb.AppendLine("    UPDATE SET Rate = Source.Rate");
            sb.AppendLine("WHEN NOT MATCHED THEN");
            sb.AppendLine(
                "    INSERT (CurrencyCode, Rate, Date) VALUES (Source.CurrencyCode, Source.Rate, Source.Date);");

            return sb.ToString();
        }
    }
}