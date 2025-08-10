using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using AlbionOnlineSniffer.Queue.Interfaces;

namespace AlbionOnlineSniffer.Queue.Publishers
{
    public class RabbitMqPublisher : IQueuePublisher, IDisposable
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

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            // Necessário para serializar System.Numerics.Vector2 (campos públicos X, Y)
            IncludeFields = true
        };

        public Task PublishAsync(string topic, object message)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message, JsonOptions);
                var body = Encoding.UTF8.GetBytes(jsonMessage);
                
                // Log detalhado para debug
                Console.WriteLine($"[RabbitMQ] Publicando: {topic}");
                Console.WriteLine($"[RabbitMQ] Mensagem: {jsonMessage}");
                Console.WriteLine($"[RabbitMQ] Tamanho: {body.Length} bytes");
                
                _channel.BasicPublish(exchange: _exchange, routingKey: topic, basicProperties: null, body: body);
                
                Console.WriteLine($"[RabbitMQ] ✅ Mensagem publicada com sucesso: {topic}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] ❌ Erro ao publicar: {ex.Message}");
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