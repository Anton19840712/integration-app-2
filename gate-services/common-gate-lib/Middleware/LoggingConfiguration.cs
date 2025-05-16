
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CommonGateLib.Middleware
{
    public static class LoggingConfiguration
    {
        public static IServiceCollection AddLoggingServices(this IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => 
                loggingBuilder.AddSerilog(dispose: true));
            return services;
        }
    }
}
