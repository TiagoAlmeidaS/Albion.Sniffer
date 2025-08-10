using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Services
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDefaultLoggingIfMissing(this IServiceCollection services)
        {
            var hasFactory = services.Any(s => s.ServiceType == typeof(ILoggerFactory));
            if (!hasFactory)
            {
                services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => { }));
            }
            return services;
        }
    }
}