
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CommonGateLib.Configuration;

namespace CommonGateLib.Middleware
{
    public static class MongoDbConfiguration
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));
            return services;
        }
    }
}
