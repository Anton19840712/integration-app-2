
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CommonGateLib.Configuration;
using CommonGateLib.RabbitMQ;

namespace CommonGateLib.Middleware
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
