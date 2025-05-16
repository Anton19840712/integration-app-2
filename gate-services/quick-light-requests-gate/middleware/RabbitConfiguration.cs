using rabbit;
using settings;

namespace middleware
{
    public static class RabbitConfiguration
    {
        public static IServiceCollection AddRabbitServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqSettings>(configuration.GetSection(nameof(RabbitMqSettings)));
            services.AddScoped<IRabbitMqService, RabbitMqService>();
            return services;
        }
    }
}
