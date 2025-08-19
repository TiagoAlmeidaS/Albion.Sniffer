using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Services;

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
			_exchange = exchange;
			_eventLogger = eventLogger ?? new NullEventLogger();

			try
			{
				var factory = new ConnectionFactory
				{
					Uri = new Uri(connectionString)
				};

				_connection = factory.CreateConnection();
				_channel = _connection.CreateModel();

				_channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);

				_eventLogger.LogEventProcessed("RabbitMQ", "Publisher inicializado", true, $"Exchange: {exchange}");
			}
			catch (Exception ex)
			{
				_eventLogger.LogCaptureError("Erro ao inicializar RabbitMQ Publisher", "Initialization", ex);
				throw;
			}
		}

		public async Task PublishAsync(string topic, object message)
		{
			try
			{
				if (_channel == null || !_channel.IsOpen)
				{
					_eventLogger.LogEventProcessed("RabbitMQ", "Publicação falhou", false, "Canal não está aberto");
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

				_eventLogger.LogEventProcessed("RabbitMQ", "Mensagem publicada", true, $"Topic: {topic}, Type: {message.GetType().Name}");
			}
			catch (Exception ex)
			{
				_eventLogger.LogCaptureError($"Erro ao publicar mensagem: {topic}", "Publish", ex);
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
				_eventLogger?.LogCaptureError("Erro ao dispor recursos RabbitMQ", "Dispose", ex);
			}
		}

		private class NullEventLogger : IAlbionEventLogger
		{
			public void AddLog(LogLevel level, string message, string? category = null, object? data = null) { }
			public IEnumerable<LogEntry> GetLogs(int count = 100, LogLevel? minLevel = null) => Enumerable.Empty<LogEntry>();
			public void LogUdpPacketCapture(byte[] payload, string sourceIp, int sourcePort, string destIp, int destPort) { }
			public void LogCaptureError(string error, string context, Exception? exception = null) { }
			public void LogNetworkDevice(string deviceName, string deviceType, bool isValid) { }
			public IEnumerable<NetworkCaptureLog> GetNetworkLogs(int count = 100, NetworkCaptureType? type = null) => Enumerable.Empty<NetworkCaptureLog>();
			public void LogEventProcessed(string eventType, object eventData, bool success, string? error = null) { }
			public void LogEventQueued(string eventType, string queueName, bool success, string? error = null) { }
			public IEnumerable<EventLog> GetEventLogs(int count = 100, string? eventType = null) => Enumerable.Empty<EventLog>();
			public LogStatistics GetStatistics() => new LogStatistics();
			public void CleanupOldLogs() { }
		}
	}
} 