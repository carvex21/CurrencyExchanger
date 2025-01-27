namespace CurrencyExchanger.Core.Models
{
    public class CurrencyRate
    {
        public string CurrencyCode { get; set; }

        public decimal Rate { get; set; }
 
        public DateTime Date { get; set; }
    }
}