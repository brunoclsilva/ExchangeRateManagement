using ExchangeRateManagement.Domain.Entities;

namespace ExchangeRateManagement.Domain.Interfaces.Services
{
    public interface ICurrencyPairIntegrationService
    {
        Task<CurrencyPair?> FetchCurrencyPair(string from, string to);
    }
}
