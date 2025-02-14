using Serilog;
using servers_api.handlers;
using servers_api.main.services;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;

namespace servers_api.middleware;

static class IntegrationConfiguration
{
	/// <summary>
	/// Регистрация API сервисов, участвующих в процессе интеграции.
	/// </summary>
	public static IServiceCollection AddApiServices(this IServiceCollection services)
	{
		Log.Information("Регистрация API-сервисов...");

		services.AddTransient<IJsonParsingService, JsonParsingService>();
		services.AddTransient<ISenderService, SenderService>();

		services.AddTransient<ITeachIntegrationService, TeachIntegrationService>();
		services.AddTransient<ITeachService, TeachService>();
		services.AddTransient<ITeachHandler, TeachHandler>();

		services.AddTransient<IStartNodeService, StartNodeService>();

		Log.Information("API-сервисы зарегистрированы.");

		return services;
	}
}
