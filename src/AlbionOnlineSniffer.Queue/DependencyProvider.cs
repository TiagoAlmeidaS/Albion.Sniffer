using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Queue.Publishers;

namespace AlbionOnlineSniffer.Queue
{
    public static class DependencyProvider
    {
        public static IQueuePublisher CreateRabbitMqPublisher(string connectionString, string exchange)
            => new RabbitMqPublisher(connectionString, exchange);

        public static IQueuePublisher CreateRedisPublisher(string connectionString)
            => new RedisPublisher(connectionString);
    }
} 