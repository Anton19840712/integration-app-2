using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;
using servers_api.factory.tcp.queuesconnections;
using servers_api.models.configurationsettings;
using servers_api.services.brokers.bpmintegration;
using servers_api.services.brokers.tcprest;

namespace servers_api.middleware
{
	static class RabbitConfiguration
	{
		/// <summary>
		/// Регистрация RabbitMQ сервисов
		/// </summary>
		public static IServiceCollection AddRabbitMqServices(this IServiceCollection services)
		{
			Log.Information("Инициализация RabbitMQ...");

			services.AddSingleton<IConnectionFactory>(provider =>
			{
				var rabbitMqSettings = provider.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

				var factory = new ConnectionFactory
				{
					HostName = rabbitMqSettings.HostName,
					Port = rabbitMqSettings.Port,
					UserName = rabbitMqSettings.UserName,
					Password = rabbitMqSettings.Password
				};

				Log.Information("RabbitMQ настроен: {Host}:{Port}", factory.HostName, factory.Port);
				return factory;
			});

			services.AddSingleton<IRabbitMqQueueListener, RabbitMqQueueListener>();
			services.AddSingleton<IRabbitMqService, RabbitMqService>();
			services.AddTransient<IRabbitMqQueueManager, RabbitMqQueueManager>();

			Log.Information("Сервисы, взаимодействующие с сетевой шиной, зарегистрированы.");

			return services;
		}
	}
}
