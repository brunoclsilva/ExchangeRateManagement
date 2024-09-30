using ExchangeRateManagement.Domain.Entities;
using ExchangeRateManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ExchangeRateManagement.Service.Services
{
    public class CurrencyPairIntegrationService : ICurrencyPairIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _apiUrl = string.Empty;
        private string _apiKey = string.Empty;
        public CurrencyPairIntegrationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _apiUrl = _configuration["Api:AlphaVantage"];
            _apiKey = _configuration["Api:ApiKey"];
        }
        public async Task<CurrencyPair?> FetchCurrencyPair(string from, string to)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}&apikey={_apiKey}&from_currency={from}&to_currency={to}");
            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadAsStringAsync();

            var parsedData = ParseApiResponse(data);
            return parsedData;
        }

        private CurrencyPair ParseApiResponse(string jsonResponse)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement.GetProperty("Realtime Currency Exchange Rate");

                string fromCurrency = root.GetProperty("1. From_Currency Code").GetString();
                string toCurrency = root.GetProperty("3. To_Currency Code").GetString();
                decimal bid = decimal.Parse(root.GetProperty("8. Bid Price").GetString());
                decimal ask = decimal.Parse(root.GetProperty("9. Ask Price").GetString());
                DateTime timestamp = DateTime.Parse(root.GetProperty("6. Last Refreshed").GetString());

                return new CurrencyPair
                {
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    Bid = bid,
                    Ask = ask,
                    Timestamp = timestamp
                };
            }
            catch (JsonException jsonEx)
            {
                throw new ApplicationException("Error parsing the API response", jsonEx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while parsing the API response", ex);
            }
        }
    }
}
