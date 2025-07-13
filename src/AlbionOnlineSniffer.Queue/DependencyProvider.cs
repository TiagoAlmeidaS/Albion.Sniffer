using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Queue.Publishers;

namespace AlbionOnlineSniffer.Queue
{
    public static class DependencyProvider
    {
        public static IQueuePublisher CreateRabbitMqPublisher(string host, string exchange)
            => new RabbitMqPublisher(host, exchange);

        public static IQueuePublisher CreateRedisPublisher(string connectionString)
            => new RedisPublisher(connectionString);
    }
} 