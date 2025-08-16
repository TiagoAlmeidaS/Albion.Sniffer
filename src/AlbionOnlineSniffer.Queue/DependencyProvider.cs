using Microsoft.Extensions.DependencyInjection;
using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Queue.Publishers;
using AlbionOnlineSniffer.Core.Interfaces;

namespace AlbionOnlineSniffer.Queue
{
    public static class DependencyProvider
    {
        /// <summary>
        /// Registra os serviços de fila
        /// </summary>
        public static void AddQueueServices(this IServiceCollection services)
        {
            services.AddSingleton<IQueuePublisher>(provider =>
            {
                var eventLogger = provider.GetService<IAlbionEventLogger>();
                var connectionString = "amqps://eioundda:CVaRCvS_mEYKl3l2uJW0dCwuMnYSycuP@cow.rmq2.cloudamqp.com/eioundda";
                var exchange = "albion.sniffer";
                return new RabbitMqPublisher(connectionString, exchange, eventLogger);
            });
        }

        /// <summary>
        /// Factory para criar RabbitMqPublisher (método legado)
        /// </summary>
        public static IQueuePublisher CreateRabbitMqPublisher(string connectionString, string exchange)
        {
            var eventLogger = new AlbionOnlineSniffer.Core.Services.AlbionEventLogger();
            return new RabbitMqPublisher(connectionString, exchange, eventLogger);
        }

        /// <summary>
        /// Factory para criar RedisPublisher (método legado)
        /// </summary>
        public static IQueuePublisher CreateRedisPublisher(string connectionString)
            => new RedisPublisher(connectionString);
    }
} 