using ExchangeRateManagement.Domain.Entities;
using ExchangeRateManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRateManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyPairController : ControllerBase
    {
        private readonly ILogger<CurrencyPairController> _logger;
        private readonly ICurrencyPairService _currencyPairService;

        public CurrencyPairController(ILogger<CurrencyPairController> logger, ICurrencyPairService currencyPairService)
        {
            _logger = logger;
            _currencyPairService = currencyPairService;
        }

        [HttpGet("{from}/{to}")]
        public async Task<ActionResult<CurrencyPair>> GetCurrencyPaisAsync(string from, string to)
        {
            _logger.LogInformation($"Getting CurrencyPair - from:{from} to:{to}");
            var currencyPair = await _currencyPairService.GetCurrencyPairAsync(from, to);

            return currencyPair;
        }

        [HttpPost]
        public async Task<ActionResult<CurrencyPair>> CreateCurrencyPaisAsync([FromBody] CurrencyPair newRate)
        {
            _logger.LogInformation($"Creating CurrencyPair - from:{newRate.FromCurrency} to:{newRate.ToCurrency}");
            var rate = await _currencyPairService.CreateCurrencyPairAsync(newRate);

            return rate;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCurrencyPaisAsync(int id, [FromBody] CurrencyPair updatedRate)
        {
            _logger.LogInformation($"Updating CurrencyPair - id:{id}");
            if (id != updatedRate.Id)
            {
                _logger.LogInformation($"Id {id} mismatched.");
                return BadRequest("ID mismatch.");
            }

            var existingRate = await _currencyPairService.GetCurrencyPairByIdAsync(id);

            if (existingRate == null)
            {
                _logger.LogInformation($"Id {id} not found.");
                return NotFound("Currency pair not found.");
            }

            await _currencyPairService.UpdateCurrencyPairAsync(id, updatedRate);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurrencyPaisAsync(int id)
        {
            _logger.LogInformation($"Deleting CurrencyPair - id:{id}");
            var currencyPair = await _currencyPairService.GetCurrencyPairByIdAsync(id);

            if (currencyPair == null)
            {
                _logger.LogInformation($"Id {id} not found.");
                return NotFound("Currency pair not found.");
            }

            await _currencyPairService.DeleteCurrencyPairAsync(id);

            return NoContent();
        }
    }
}
