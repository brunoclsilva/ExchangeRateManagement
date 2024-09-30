using Moq;
using System.Net;
using Microsoft.Extensions.Configuration;
using ExchangeRateManagement.Service.Services;
using Moq.Protected;

namespace ExchangeRateManagement.Test.Services
{
    public class CurrencyPairIntegrationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly CurrencyPairIntegrationService _service;

        public CurrencyPairIntegrationServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Api:AlphaVantage"]).Returns("https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE");
            _configurationMock.Setup(c => c["Api:ApiKey"]).Returns("dummyApiKey");

            _service = new CurrencyPairIntegrationService(_httpClient, _configurationMock.Object);
        }

        [Fact]
        public async Task FetchCurrencyPair_ReturnsCurrencyPair_WhenApiResponseIsSuccessful()
        {
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
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var result = await _service.FetchCurrencyPair("USD", "EUR");

            Assert.NotNull(result);
            Assert.Equal("USD", result.FromCurrency);
            Assert.Equal("EUR", result.ToCurrency);
            Assert.Equal(1.2000m, result.Bid);
            Assert.Equal(1.2100m, result.Ask);
            Assert.Equal(new DateTime(2024, 9, 27, 12, 30, 0), result.Timestamp);
        }

        [Fact]
        public async Task FetchCurrencyPair_ReturnsNull_WhenApiResponseIsUnsuccessful()
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var result = await _service.FetchCurrencyPair("USD", "EUR");

            Assert.Null(result);
        }

        [Fact]
        public async Task ParseApiResponse_ThrowsApplicationException_OnJsonError()
        {
            var invalidJsonResponse = @"{""bad json""}";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidJsonResponse)
                });

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _service.FetchCurrencyPair("USD", "EUR"));
            Assert.Contains("Error parsing the API response", exception.Message);
        }

        [Fact]
        public async Task ParseApiResponse_ThrowsApplicationException_OnGeneralError()
        {
            var invalidJsonResponse = @"{}";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>() 
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidJsonResponse)
                });

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _service.FetchCurrencyPair("USD", "EUR"));
            Assert.Contains("An error occurred while parsing the API response", exception.Message);
        }
    }
}
