
using Microsoft.Extensions.DependencyInjection;

namespace CommonGateLib.Middleware
{
    public static class CommonConfiguration
    {
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
