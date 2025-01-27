namespace CurrencyExchanger.Api.Models;

public class AdjustBalanceRequest
{
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EUR";

    public string Strategy { get; set; }
}