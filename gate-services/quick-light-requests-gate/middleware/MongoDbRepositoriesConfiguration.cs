using repositories;

namespace middleware
{
	public static class MongoDbRepositoriesConfiguration
	{
		public static IServiceCollection AddMongoDbRepositoriesServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));
			return services;
		}
	}
}
