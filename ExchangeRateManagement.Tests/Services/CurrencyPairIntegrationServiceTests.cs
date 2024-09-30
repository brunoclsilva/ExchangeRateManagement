using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ExchangeRateManagement.Domain.Entities;
using ExchangeRateManagement.Service.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;

namespace ExchangeRateManagement.Tests.Services
{
    [TestClass]
    public class CurrencyPairIntegrationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly CurrencyPairIntegrationService _service;

        public CurrencyPairIntegrationServiceTests()
        {
            // Setup HttpClient mock
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            // Setup IConfiguration mock
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Api:AlphaVantage"]).Returns("https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE");
            _configurationMock.Setup(c => c["Api:ApiKey"]).Returns("dummyApiKey");

            // Instantiate the service with mocks
            _service = new CurrencyPairIntegrationService(_httpClient, _configurationMock.Object);
        }

        [Fact]
        public async Task FetchCurrencyPair_Returns_CurrencyPair_When_ApiResponse_Is_Successful()
        {
            // Arrange: Setup a successful HTTP response
            var jsonResponse = @"
            {
                ""Realtime Currency Exchange Rate"": {
                    ""1. From_Currency Code"": ""USD"",
                    ""3. To_Currency Code"": ""EUR"",
                    ""8. Bid Price"": ""1.2000"",
                    ""9. Ask Price"": ""1.2100"",
                    ""6. Last Refreshed"": ""2024-09-27 12:30:00""
                }
            }";

            _httpMessageHandlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act: Call the service
            var result = await _service.FetchCurrencyPair("USD", "EUR");

            // Assert: Verify the result
            Assert.NotNull(result);
            Assert.Equal("USD", result.FromCurrency);
            Assert.Equal("EUR", result.ToCurrency);
            Assert.Equal(1.2000m, result.Bid);
            Assert.Equal(1.2100m, result.Ask);
            Assert.Equal(new DateTime(2024, 9, 27, 12, 30, 0), result.Timestamp);
        }

        [Fact]
        public async Task FetchCurrencyPair_Returns_Null_When_ApiResponse_Is_Unsuccessful()
        {
            // Arrange: Setup a failed HTTP response
            _httpMessageHandlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act: Call the service
            var result = await _service.FetchCurrencyPair("USD", "EUR");

            // Assert: Verify the result is null
            Assert.Null(result);
        }

        [Fact]
        public async Task ParseApiResponse_Throws_ApplicationException_On_JsonError()
        {
            // Arrange: Malformed JSON response
            var invalidJsonResponse = @"{""bad json""}";

            _httpMessageHandlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidJsonResponse)
                });

            // Act & Assert: Ensure it throws an ApplicationException due to JsonException
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _service.FetchCurrencyPair("USD", "EUR"));
            Assert.Contains("Error parsing the API response", exception.Message);
        }

        [Fact]
        public async Task ParseApiResponse_Throws_ApplicationException_On_GeneralError()
        {
            // Arrange: Force the HttpClient to return an empty response that will cause a general parsing error
            var invalidJsonResponse = @"{}"; // Missing the expected "Realtime Currency Exchange Rate" object

            _httpMessageHandlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidJsonResponse)
                });

            // Act & Assert: Ensure it throws an ApplicationException due to general parsing error
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _service.FetchCurrencyPair("USD", "EUR"));
            Assert.Contains("An error occurred while parsing the API response", exception.Message);
        }
    }
}
