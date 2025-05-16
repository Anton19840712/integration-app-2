
using Microsoft.Extensions.DependencyInjection;

namespace common_gate_lib.Configuration
{
    public static class MongoDbConfiguration 
    {
        public static IServiceCollection AddMongoDbServices(this IServiceCollection services)
        {
            services.AddSingleton<IMongoRepository, MongoRepository>();
            return services;
        }
    }
}
