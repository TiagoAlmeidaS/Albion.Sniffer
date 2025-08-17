using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AlbionOnlineSniffer.Queue.Interfaces;
using AlbionOnlineSniffer.Queue.Publishers;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Core.Services;
using Microsoft.Extensions.Logging;

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

            // Bridge padrão (usando configuração acima)
            services.AddSingleton<EventToQueueBridge>(sp =>
            {
                var dispatcher = sp.GetRequiredService<AlbionOnlineSniffer.Core.Services.EventDispatcher>();
                var publisher = sp.GetRequiredService<IQueuePublisher>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new EventToQueueBridge(dispatcher, publisher, loggerFactory.CreateLogger<EventToQueueBridge>());
            });
        }

        /// <summary>
        /// Registra os serviços de fila lendo configurações
        /// RabbitMQ:ConnectionString, RabbitMQ:Exchange ou ConnectionStrings:RabbitMQ
        /// </summary>
        public static void AddQueueServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IQueuePublisher>(provider =>
            {
                var eventLogger = provider.GetService<IAlbionEventLogger>();
                var connectionString = configuration.GetConnectionString("RabbitMQ")
                    ?? configuration.GetValue<string>("RabbitMQ:ConnectionString")
                    ?? configuration.GetValue<string>("Queue:ConnectionString")
                    ?? "amqp://localhost";
                var exchange = configuration.GetValue<string>("RabbitMQ:Exchange")
                    ?? configuration.GetValue<string>("Queue:ExchangeName")
                    ?? "albion.sniffer";
                return new RabbitMqPublisher(connectionString, exchange, eventLogger);
            });

            services.AddSingleton<EventToQueueBridge>(sp =>
            {
                var dispatcher = sp.GetRequiredService<AlbionOnlineSniffer.Core.Services.EventDispatcher>();
                var publisher = sp.GetRequiredService<IQueuePublisher>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new EventToQueueBridge(dispatcher, publisher, loggerFactory.CreateLogger<EventToQueueBridge>());
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