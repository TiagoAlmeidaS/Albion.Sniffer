using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Queue.Publishers
{
    public class RabbitMqPublisher : IQueuePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;
        private readonly IAlbionEventLogger _eventLogger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            IncludeFields = true
        };

        public RabbitMqPublisher(string connectionString, string exchange, IAlbionEventLogger? eventLogger = null)
        {
            _eventLogger = eventLogger ?? new AlbionOnlineSniffer.Core.Services.AlbionEventLogger();
            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            _connection = factory.CreateConnection();
            _channel = factory.CreateModel();
            _exchange = exchange;

            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
        }

        public Task PublishAsync(string topic, object message)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message, JsonOptions);
                var body = Encoding.UTF8.GetBytes(jsonMessage);
                
                _channel.BasicPublish(exchange: _exchange, routingKey: topic, basicProperties: null, body: body);
                
                // Log do evento enviado para fila
                _eventLogger.LogEventQueued(topic, "RabbitMQ", true);
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Log do erro ao enviar para fila
                _eventLogger.LogEventQueued(topic, "RabbitMQ", false, ex.Message);
                return Task.FromException(ex);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
} 