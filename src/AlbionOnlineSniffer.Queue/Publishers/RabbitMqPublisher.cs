using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using AlbionOnlineSniffer.Queue.Interfaces;

namespace AlbionOnlineSniffer.Queue.Publishers
{
    public class RabbitMqPublisher : IQueuePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public RabbitMqPublisher(string connectionString, string exchange)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(connectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _exchange = exchange;
            _channel.ExchangeDeclare(exchange: _exchange, type: "topic", durable: true);
        }

        public Task PublishAsync(string topic, object message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _channel.BasicPublish(exchange: _exchange, routingKey: topic, basicProperties: null, body: body);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
} 