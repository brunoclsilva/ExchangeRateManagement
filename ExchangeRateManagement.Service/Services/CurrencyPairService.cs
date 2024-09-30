using ExchangeRateManagement.Domain.Entities;
using ExchangeRateManagement.Domain.Interfaces.Services;
using ExchangeRateManagement.Infra.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateManagement.Service.Services
{
    public class CurrencyPairService : ICurrencyPairService
    {
        private readonly ExchangeRateContext _context;
        private readonly ICurrencyPairIntegrationService _currencyPairIntegrationService;
        private readonly IMessagingService _messagingService;

        public CurrencyPairService(ExchangeRateContext context, ICurrencyPairIntegrationService currencyPairIntegrationService, IMessagingService messagingService)
        {
            _context = context;
            _currencyPairIntegrationService = currencyPairIntegrationService;
            _messagingService = messagingService;
        }

        public async Task<CurrencyPair> CreateCurrencyPairAsync(CurrencyPair currencyPair)
        {

            _context.CurrencyPairs.Add(currencyPair);
            await _context.SaveChangesAsync();

            return currencyPair;
        }

        public async Task DeleteCurrencyPairAsync(int id)
        {
            var existingCurrencyPair = await GetCurrencyPairByIdAsync(id);

            if (existingCurrencyPair != null)
            {
                _context.CurrencyPairs.Remove(existingCurrencyPair);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CurrencyPair> GetCurrencyPairAsync(string from, string to)
        {
            var currencyPair = await _context.CurrencyPairs
                .OrderByDescending(o => o.Timestamp)
                .FirstOrDefaultAsync(c => c.FromCurrency == from && c.ToCurrency == to);

            if (currencyPair == null)
            {
                var fetchedRate = await _currencyPairIntegrationService.FetchCurrencyPair(from, to);
                if (fetchedRate != null)
                {
                    _context.CurrencyPairs.Add(fetchedRate);
                    await _context.SaveChangesAsync();

                    await _messagingService.PublishAddedCurrencyPairAsync(fetchedRate);

                    return fetchedRate;
                }
            }

            return currencyPair;
        }

        public async Task<CurrencyPair?> GetCurrencyPairByIdAsync(int id)
        {
            var currencyPair = await _context.CurrencyPairs.FindAsync(id);

            return currencyPair;
        }

        public async Task UpdateCurrencyPairAsync(int id, CurrencyPair currencyPair)
        {
            var existingCurrencyPair = await GetCurrencyPairByIdAsync(id); 

            if (existingCurrencyPair != null)
            {
                existingCurrencyPair.Bid = currencyPair.Bid;
                existingCurrencyPair.Ask = currencyPair.Ask;
                existingCurrencyPair.Timestamp = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }
    }
}
