using CurrencyExchanger.Core.Models;
using CurrencyExchanger.Infrastructure;
using CurrencyExchanger.Infrastructure.GatewayLibrary;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchanger.Api.Services
{
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrencyGateway _currencyGateway;

        public WalletService(AppDbContext dbContext, ICurrencyGateway currencyGateway)
        {
            _dbContext = dbContext;
            _currencyGateway = currencyGateway;
        }

        public async Task<Wallet> CreateWalletAsync()
        {
            var wallet = new Wallet
            {
                Currency = "EUR",
                Balance = 0
            };

            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            return wallet;
        }

        public async Task<Wallet> GetWalletAsync(long walletId)
        {
            return await _dbContext.Wallets.FirstOrDefaultAsync(w => w.Id == walletId)
                ?? throw new Exception(">>Wallet not found<<");
        }

        public async Task<decimal> GetConvertedBalanceAsync(long walletId, string? targetCurrency)
        {
            var wallet = await GetWalletAsync(walletId);

            decimal sourceRate = 1;
            decimal targetRate = 1;

            if (wallet.Currency != "EUR")
            {
                var rates = await _currencyGateway.GetCurrencyRatesAsync();

                var sourceCurrencyRate = rates.FirstOrDefault(r => r.CurrencyCode == wallet.Currency);
                if (sourceCurrencyRate == null)
                {
                    throw new ArgumentException($">>The wallet currency '{wallet.Currency}' is not supported<<");
                }

                sourceRate = sourceCurrencyRate.Rate;
            }

            if (targetCurrency != "EUR")
            {
                var rates = await _currencyGateway.GetCurrencyRatesAsync();

                var targetCurrencyRate = rates.FirstOrDefault(r => r.CurrencyCode == targetCurrency);
                if (targetCurrencyRate == null)
                {
                    throw new ArgumentException($">>The target currency '{targetCurrency}' is not supported<<");
                }

                targetRate = targetCurrencyRate.Rate;
            }

            return wallet.Balance / sourceRate * targetRate;
        }

        public async Task AdjustBalanceAsync(long walletId, decimal amount, string currency, string strategy)
        {
            var wallet = await GetWalletAsync(walletId);

            decimal sourceRate = 1;
            decimal targetRate = 1;

            if (currency != "EUR")
            {
                var rates = await _currencyGateway.GetCurrencyRatesAsync();

                var sourceCurrencyRate = rates.FirstOrDefault(r => r.CurrencyCode == currency);
                if (sourceCurrencyRate == null)
                {
                    throw new ArgumentException($">>The currency '{currency}' is not supported<<");
                }

                sourceRate = sourceCurrencyRate.Rate;
            }

            if (wallet.Currency != "EUR")
            {
                var rates = await _currencyGateway.GetCurrencyRatesAsync();

                var targetCurrencyRate = rates.FirstOrDefault(r => r.CurrencyCode == wallet.Currency);
                if (targetCurrencyRate == null)
                {
                    throw new ArgumentException($">>The wallet currency '{wallet.Currency}' is not supported<<");
                }

                targetRate = targetCurrencyRate.Rate;
            }
            
            amount = amount / sourceRate * targetRate;

            switch (strategy.ToLower())
            {
                case "addfundsstrategy":
                    wallet.Balance += amount;
                    break;

                case "subtractfundsstrategy":
                    if (wallet.Balance < amount)
                        throw new Exception(">>Insufficient funds<<");
                    wallet.Balance -= amount;
                    break;

                case "forcesubtractfundsstrategy":
                    wallet.Balance -= amount;
                    break;

                default:
                    throw new Exception(">>Unknown strategy<<");
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
