using Serilog;
using servers_api.factory.abstractions;
using servers_api.factory.tcp.instancehandlers;
using servers_api.factory.tcp.instances;

namespace servers_api.middleware
{
	static class ProtocolProcessingConfiguration
	{
		/// <summary>
		/// Регистрация factory сервисов
		/// </summary>
		public static IServiceCollection AddFactoryServices(this IServiceCollection services)
		{
			Log.Information("Регистрация factory сервисов...");

			services.AddScoped<IProtocolManager, ProtocolManager>();
			services.AddTransient<TcpFactory>();
			services.AddScoped<TcpServer>();
			services.AddScoped<TcpClient>();
			services.AddScoped<ITcpServerHandler, TcpServerHandler>();

			Log.Information("Factory сервисы зарегистрированы.");

			return services;
		}
	}
}
