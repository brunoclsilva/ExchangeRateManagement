using ExchangeRateManagement.Domain.Entities;
using ExchangeRateManagement.Domain.Interfaces.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace ExchangeRateManagement.Service.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly IConnectionFactory _connectionFactory;

        public MessagingService(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task PublishAddedCurrencyPairAsync(CurrencyPair currencyPair)
        {
            var factory = _connectionFactory;
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "currencyPairAddedQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var message = JsonConvert.SerializeObject(currencyPair);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "currencyPairAddedQueue", basicProperties: null, body: body);
        }
    }
}
