using System.ComponentModel.DataAnnotations;

namespace CurrencyExchanger.Core.Models
{
    public class Wallet
    {
        public long Id { get; set; }

        [Required]
        public decimal Balance { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; }
    }
}