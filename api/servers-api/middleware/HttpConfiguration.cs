using Serilog;

namespace servers_api.middleware
{
	static class HttpConfiguration
	{
		public static IServiceCollection AddHttoServices(this IServiceCollection services)
		{
			Log.Information("Регистрация http сервисов...");

			services.AddHttpClient();
			services.AddHttpContextAccessor();

			Log.Information("Http сервисы зарегистрированы.");

			return services;
		}
	}
}
