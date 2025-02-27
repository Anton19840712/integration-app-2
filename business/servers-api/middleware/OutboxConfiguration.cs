using Serilog;
using servers_api.models.entities;
using servers_api.models.outbox;
using servers_api.repositories;

namespace servers_api.middleware;

/// <summary>
/// Класс обслуживает логику паттерна outbox.
/// </summary>
public static class OutboxConfiguration
{
	public static IServiceCollection AddOutboxServices(this IServiceCollection services)
	{
		Log.Information("Регистрация OutboxProcessor...");

		// Регистрируем как IHostedService для фонового выполнения

		services.AddHostedService<OutboxMongoBackgroundService>();
		services.AddSingleton<MongoRepository<OutboxMessage>>();
		services.AddSingleton<MongoRepository<QueuesEntity>>();

		Log.Information("OutboxProcessor зарегистрирован.");
		return services;
	}
}
