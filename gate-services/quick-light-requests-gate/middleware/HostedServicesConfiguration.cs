using background;

namespace middleware
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
