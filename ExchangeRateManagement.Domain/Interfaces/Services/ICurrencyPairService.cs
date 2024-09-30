using ExchangeRateManagement.Domain.Entities;

namespace ExchangeRateManagement.Domain.Interfaces.Services
{
    public interface ICurrencyPairService
    {
        Task<CurrencyPair> GetCurrencyPairAsync(string from, string to);
        Task<CurrencyPair?> GetCurrencyPairByIdAsync(int id);
        Task<CurrencyPair> CreateCurrencyPairAsync(CurrencyPair currencyPair);
        Task UpdateCurrencyPairAsync(int id, CurrencyPair currencyPair);
        Task DeleteCurrencyPairAsync(int currencyPair);
    }
}
