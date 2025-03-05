using Serilog;
using servers_api.factory;
using servers_api.main.services;
using servers_api.protocols.http;
using servers_api.protocols.tcp;
using servers_api.protocols.udp;

namespace servers_api.middleware;

static class ProtocolProcessingConfiguration
{
	public static IServiceCollection AddFactoryServices(this IServiceCollection services)
	{
		Log.Information("Регистрация factory сервисов...");

		// Менеджер протоколов
		services.AddTransient<IProtocolManager, ProtocolManager>();

		// Добавляем зависимые классы, которые используются в фабриках
		services.AddTransient<TcpClientInstance>();
		services.AddTransient<TcpServerInstance>();

		services.AddTransient<UdpClientInstance>();
		services.AddTransient<UdpServerInstance>();

		services.AddTransient<HttpClientInstance>();
		services.AddTransient<HttpServerInstance>();

		// Регистрируем фабрики с областью жизни Scoped
		services.AddScoped<TcpFactory>();
		services.AddScoped<UdpFactory>();
		services.AddScoped<HttpFactory>();

		// Регистрируем словарь с фабриками как Singleton
		services.AddSingleton(provider =>
		{
			return new Dictionary<string, UpInstanceByProtocolFactory>
			{
				{ "tcp", provider.GetRequiredService<TcpFactory>() },
				{ "udp", provider.GetRequiredService<UdpFactory>() },
				{ "http", provider.GetRequiredService<HttpFactory>() }
			};
		});

		// Основные сервисы:
		services.AddTransient<ITeachIntegrationService, TeachIntegrationService>();

		Log.Information("Factory сервисы зарегистрированы.");
		return services;
	}
}
