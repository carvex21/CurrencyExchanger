using CurrencyExchanger.Core.Models;

namespace CurrencyExchanger.Api.Services;

public interface IWalletService
{
    Task<Wallet> CreateWalletAsync();
    Task<Wallet> GetWalletAsync(long walletId);
    Task<decimal> GetConvertedBalanceAsync(long walletId, string targetCurrency);
    Task AdjustBalanceAsync(long walletId, decimal amount, string currency, string strategy);
}
