using System.ComponentModel.DataAnnotations;

namespace ExchangeRateManagement.Domain.Entities
{
    public class CurrencyPair
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "FromCurrency is required.")]
        public string? FromCurrency { get; set; }
        [Required(ErrorMessage = "ToCurrency is required.")]
        public string? ToCurrency { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
