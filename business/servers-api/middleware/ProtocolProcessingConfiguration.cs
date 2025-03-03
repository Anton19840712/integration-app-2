using Serilog;
using servers_api.factory.abstractions;
using servers_api.factory.udp;
using servers_api.main.facades;
using servers_api.main.services;

namespace servers_api.middleware;

static class ProtocolProcessingConfiguration
{
	/// <summary>
	/// Регистрация factory сервисов при создании факторов tcp.
	/// </summary>
	public static IServiceCollection AddFactoryServices(this IServiceCollection services)
	{
		Log.Information("Регистрация factory сервисов...");

		services.AddTransient<IProtocolManager, ProtocolManager>();
		services.AddTransient<UpInstanceByProtocolFactory, TcpFactory>();

		// main services:
		services.AddTransient<IStartNodeService, StartNodeService>();
		services.AddTransient<ITeachIntegrationService, TeachIntegrationService>();

		// main facades:
		services.AddTransient<IIntegrationFacade, IntegrationFacade>();
		services.AddTransient<IQueueFacade, QueueFacade>();
		services.AddTransient<IProcessingFacade, ProcessingFacade>();

		// tcp:
		services.AddTransient<UdpServerInstance>();
		services.AddTransient<TcpClientInstance>();

		Log.Information("Factory сервисы зарегистрированы.");

		return services;
	}
}
