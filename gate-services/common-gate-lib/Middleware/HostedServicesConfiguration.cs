
using Microsoft.Extensions.DependencyInjection;
using CommonGateLib.Background;

namespace CommonGateLib.Middleware
{
    public static class HostedServicesConfiguration
    {
        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<OutboxMongoBackgroundService>();
            services.AddHostedService<QueueListenerBackgroundService>();
            return services;
        }
    }
}
