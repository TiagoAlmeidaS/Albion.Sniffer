using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using AlbionOnlineSniffer.Queue.Interfaces;

namespace AlbionOnlineSniffer.Queue.Publishers
{
	public class RabbitMqPublisher : IQueuePublisher, IDisposable
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly string _exchange;
		        private readonly object _eventLogger;

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = false,
			IncludeFields = true
		};

		public RabbitMqPublisher(string connectionString, string exchange, object? eventLogger = null)
        {
            _exchange = exchange;
            _eventLogger = eventLogger ?? new object();

			try
			{
				var factory = new ConnectionFactory
				{
					Uri = new Uri(connectionString)
				};

				_connection = factory.CreateConnection();
				_channel = _connection.CreateModel();

				_channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task PublishAsync(string topic, object message)
		{
			try
			{
				if (_channel == null || !_channel.IsOpen)
				{
					return;
				}

				var json = JsonSerializer.Serialize(message, JsonOptions);
				var body = Encoding.UTF8.GetBytes(json);

				var properties = _channel.CreateBasicProperties();
				properties.Persistent = true;
				properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

				_channel.BasicPublish(
					exchange: _exchange,
					routingKey: topic,
					mandatory: true,
					basicProperties: properties,
					body: body);

				                // _eventLogger.LogEventProcessed("RabbitMQ", "Mensagem publicada", true, $"Topic: {topic}, Type: {message.GetType().Name}");
			}
			catch (Exception ex)
			{
				                // _eventLogger.LogCaptureError($"Erro ao publicar mensagem: {topic}", "Publish", ex);
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				_channel?.Close();
				_channel?.Dispose();
				_connection?.Close();
				_connection?.Dispose();
			}
			catch (Exception ex)
			{
				                // _eventLogger?.LogCaptureError("Erro ao dispor recursos RabbitMQ", "Dispose", ex);
			}
		}


	}
}
