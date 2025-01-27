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
        private readonly IServiceProvider _serviceProvider;
        private readonly CurrencyRateCache _cache;
        private readonly ILogger<CurrencyUpdateJob> _logger;
        private readonly TimeSpan _refreshFrequency;

        public CurrencyUpdateJob(IServiceProvider serviceProvider, CurrencyRateCache cache,
            ILogger<CurrencyUpdateJob> logger, IOptions<EcbWorker> options)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
            _logger = logger;
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

                    // Use a scoped service to fetch rates
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var gateway = scope.ServiceProvider.GetRequiredService<CurrencyGateway>();
                        var rates = await gateway.GetCurrencyRatesAsync();

                        if (rates.Any())
                        {
                            // Update the shared cache
                            _cache.UpdateRates(rates);
                            _logger.LogInformation("++Cache updated with {Count} rates++", rates.Count());

                            // Update the database
                            await UpdateDatabaseAsync(rates, scope.ServiceProvider);
                        }
                        else
                        {
                            _logger.LogWarning(">>No currency rates were fetched<<");
                        }
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

        private async Task UpdateDatabaseAsync(IEnumerable<CurrencyRate> rates, IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("~~Updating database with fresh currency rates~~");

            // Generate and execute MERGE SQL
            var mergeCommand = BuildMergeCommand(rates);
            await dbContext.Database.ExecuteSqlRawAsync(mergeCommand);

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