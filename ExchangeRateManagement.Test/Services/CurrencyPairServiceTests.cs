using Microsoft.EntityFrameworkCore;
using Moq;
using ExchangeRateManagement.Service.Services;
using ExchangeRateManagement.Domain.Entities;
using ExchangeRateManagement.Infra.Repositories;
using ExchangeRateManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace ExchangeRateManagement.Tests.Services
{
    public class CurrencyPairServiceTests
    {
        private readonly ExchangeRateContext _context;
        private readonly Mock<ICurrencyPairIntegrationService> _integrationServiceMock;
        private readonly Mock<IMessagingService> _messagingServiceMock;
        private readonly CurrencyPairService _service;
        private readonly Mock<IConfiguration> _configurationMock;

        public CurrencyPairServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            
            var options = new DbContextOptionsBuilder<ExchangeRateContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ExchangeRateContext(options, _configurationMock.Object);

            _integrationServiceMock = new Mock<ICurrencyPairIntegrationService>();
            _messagingServiceMock = new Mock<IMessagingService>();

            _service = new CurrencyPairService(_context, _integrationServiceMock.Object, _messagingServiceMock.Object);
        }

        [Fact]
        public async Task CreateCurrencyPairAsync_AddsCurrencyPair_SavesChanges()
        {
            var currencyPair = new CurrencyPair { FromCurrency = "USD", ToCurrency = "EUR", Bid = 1.1m, Ask = 1.2m, Timestamp = DateTime.Now };

            await _service.CreateCurrencyPairAsync(currencyPair);

            var savedCurrencyPair = await _context.CurrencyPairs.FindAsync(currencyPair.Id);
            Assert.NotNull(savedCurrencyPair);
            Assert.Equal(currencyPair.FromCurrency, savedCurrencyPair.FromCurrency);
            Assert.Equal(currencyPair.ToCurrency, savedCurrencyPair.ToCurrency);
            Assert.Equal(currencyPair.Bid, savedCurrencyPair.Bid);
            Assert.Equal(currencyPair.Ask, savedCurrencyPair.Ask);
        }

        [Fact]
        public async Task DeleteCurrencyPairAsync_RemovesCurrencyPair_IfExists()
        {
            var currencyPair = new CurrencyPair { Id = 999, FromCurrency = "USD", ToCurrency = "EUR", Bid = 1.1m, Ask = 1.2m, Timestamp = DateTime.Now };
            _context.CurrencyPairs.Add(currencyPair);
            await _context.SaveChangesAsync();

            await _service.DeleteCurrencyPairAsync(1);

            var result = await _context.CurrencyPairs.FindAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteCurrencyPairAsync_DoesNothing_IfNotFound()
        {
            await _service.DeleteCurrencyPairAsync(1);

            var result = await _context.CurrencyPairs.FindAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrencyPairAsync_ReturnsCurrencyPair_FromDatabase()
        {
            var currencyPair = new CurrencyPair { FromCurrency = "USD", ToCurrency = "EUR", Bid = 1.1m, Ask = 1.2m, Timestamp = DateTime.Now };
            _context.CurrencyPairs.Add(currencyPair);
            await _context.SaveChangesAsync();

            var result = await _service.GetCurrencyPairAsync("USD", "EUR");

            Assert.Equal(currencyPair.FromCurrency, result.FromCurrency);
            Assert.Equal(currencyPair.ToCurrency, result.ToCurrency);
        }

        [Fact]
        public async Task GetCurrencyPairAsync_FetchesFromApi_IfNotFoundInDatabase()
        {
            var fetchedPair = new CurrencyPair { FromCurrency = "USD", ToCurrency = "EUR", Bid = 1.3m, Ask = 1.4m, Timestamp = DateTime.Now };
            _integrationServiceMock.Setup(c => c.FetchCurrencyPair("USD", "EUR")).ReturnsAsync(fetchedPair);

            var result = await _service.GetCurrencyPairAsync("USD", "EUR");

            Assert.Equal(fetchedPair.FromCurrency, result.FromCurrency);
            Assert.Equal(fetchedPair.ToCurrency, result.ToCurrency);
            _messagingServiceMock.Verify(m => m.PublishAddedCurrencyPairAsync(fetchedPair), Times.Once);
        }

        [Fact]
        public async Task UpdateCurrencyPairAsync_UpdatesExistingCurrencyPair()
        {
            var existingCurrencyPair = new CurrencyPair { Id = 99, FromCurrency = "USD", ToCurrency = "EUR", Bid = 1.1m, Ask = 1.2m, Timestamp = DateTime.Now.AddHours(-1) };
            var updatedCurrencyPair = new CurrencyPair { Id = 99, Bid = 1.3m, Ask = 1.4m };

            _context.CurrencyPairs.Add(existingCurrencyPair);
            await _context.SaveChangesAsync();

            await _service.UpdateCurrencyPairAsync(99, updatedCurrencyPair);

            var result = await _context.CurrencyPairs.FindAsync(99);
            Assert.Equal(1.3m, result.Bid);
            Assert.Equal(1.4m, result.Ask);
        }
    }
}
