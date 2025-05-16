
using Microsoft.Extensions.DependencyInjection;
using CommonGateLib.Repository;

namespace CommonGateLib.Middleware
{
    public static class MongoDbRepositoriesConfiguration
    {
        public static IServiceCollection AddMongoRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
            return services;
        }
    }
}
