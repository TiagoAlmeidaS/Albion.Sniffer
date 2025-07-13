using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using AlbionOnlineSniffer.Queue.Interfaces;

namespace AlbionOnlineSniffer.Queue.Publishers
{
    public class RedisPublisher : IQueuePublisher
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ISubscriber _subscriber;

        public RedisPublisher(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _subscriber = _redis.GetSubscriber();
        }

        public Task PublishAsync(string topic, object message)
        {
            var json = JsonSerializer.Serialize(message);
            return _subscriber.PublishAsync(topic, json);
        }

        public void Dispose()
        {
            _redis?.Dispose();
        }
    }
} 