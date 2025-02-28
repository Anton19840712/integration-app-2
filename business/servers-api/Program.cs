using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using servers_api.api.minimal;
using servers_api.middleware;
using servers_api.models.configurationsettings;
using servers_api.models.entities;
using servers_api.models.outbox;
using servers_api.repositories;
using servers_api.services;
using servers_api.services.brokers.bpmintegration;

Console.Title = "integration api";

var builder = WebApplication.CreateBuilder(args);
var cts = new CancellationTokenSource();

// Настройка логирования
builder.Host.UseSerilog((ctx, cfg) =>
{
	cfg.WriteTo
		.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
		.Enrich
		.FromLogContext();
});

try
{
	var services = builder.Services;
	var configuration = builder.Configuration;

	services.AddControllers();

	services.AddCommonServices();
	services.AddHttpServices();
	services.AddFactoryServices();
	services.AddApiServices();
	services.AddRabbitMqServices(configuration);
	services.AddMessageServingServices();
	services.AddMongoDbServices(configuration);
	services.AddAutoMapper(typeof(MappingProfile));
	services.AddOutboxServices();
	services.AddValidationServices();

	services.AddSingleton<IRabbitMqQueueListener, RabbitMqQueueListener>();

	services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

	// Разбор зависимостей для репозиториев:
	builder.Services.AddTransient<IMongoRepository<QueuesEntity>, MongoRepository<QueuesEntity>>();
	builder.Services.AddTransient<IMongoRepository<OutboxMessage>, MongoRepository<OutboxMessage>>();
	builder.Services.AddTransient<IMongoRepository<IncidentEntity>, MongoRepository<IncidentEntity>>();

	services.AddSingleton<MongoRepository<OutboxMessage>>();
	services.AddSingleton<MongoRepository<QueuesEntity>>();
	services.AddSingleton<MongoRepository<IncidentEntity>>();

	//services.AddHostedService<QueueListenerBackgroundService>();
	//builder.Services.AddScoped<IHostedService, QueueListenerBackgroundService>();

	builder.Services.AddSingleton<IMongoClient>(sp =>
	{
		var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
		return new MongoClient(settings.ConnectionString);
	});

	builder.Services.AddSingleton<IMongoDatabase>(sp =>
	{
		var mongoClient = sp.GetRequiredService<IMongoClient>();
		var databaseName = builder.Configuration["MongoDbSettings:DatabaseName"];
		return mongoClient.GetDatabase(databaseName);
	});

	var app = builder.Build();

	app.UseSerilogRequestLogging();
	app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

	var factory = app.Services.GetRequiredService<ILoggerFactory>();

	app.MapControllers();
	app.MapIntegrationMinimalApi(factory);
	app.MapAdminMinimalApi(factory);

	Log.Information("Динамический шлюз запущен и готов к эксплуатации.");

	//using (var scope = app.Services.CreateScope())
	//{
	//	var integrationFacade = scope.ServiceProvider.GetRequiredService<IIntegrationFacade>();
	//	var queuesRepository = scope.ServiceProvider.GetRequiredService<MongoRepository<QueuesEntity>>();

	//	var logger = factory.CreateLogger("AdminEndpoints");

	//	// Логика старта слушателей
	//	try
	//	{
	//		logger.LogInformation("Dumping messages from all queues.");

	//		// Получаем названия всех очередей из репозитория:
	//		var elements = await queuesRepository.GetAllAsync();

	//		foreach (var element in elements)
	//		{
	//			try
	//			{
	//				// Для каждой очереди запускаем слушателя:
	//				await integrationFacade.StartListeningAsync(element.OutQueueName, cts.Token);
	//			}
	//			catch (Exception ex)
	//			{
	//				// Логируем ошибку для каждой очереди отдельно, но продолжаем обработку других:
	//				logger.LogError(ex, "Error retrieving messages from queue: {QueueName}", element.OutQueueName);
	//			}
	//		}

	//		logger.LogInformation("Процесс получения сообщений из очередей завершен.");
	//	}
	//	catch (Exception ex)
	//	{
	//		logger.LogError(ex, "Error while getting messages from queues");
	//	}
	//}

	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Критическая ошибка при запуске приложения");
	throw;
}
finally
{
	cts.Cancel();
	cts.Dispose();
	Log.CloseAndFlush();
}
