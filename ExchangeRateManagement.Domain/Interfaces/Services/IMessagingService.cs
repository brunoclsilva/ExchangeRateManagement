using ExchangeRateManagement.Domain.Entities;

namespace ExchangeRateManagement.Domain.Interfaces.Services
{
    public interface IMessagingService
    {
        Task PublishAddedCurrencyPairAsync(CurrencyPair currencyPair);
    }
}
