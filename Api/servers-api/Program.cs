using Serilog;
using servers_api.api.minimalapi;
using servers_api.middleware;
using servers_api.repositories;

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
	//GateConfiguration.ConfigureDynamicGate(args, builder);

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

	var app = builder.Build();

	app.UseSerilogRequestLogging();
	app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

	var factory = app.Services.GetRequiredService<ILoggerFactory>();

	app.MapControllers();
	app.MapCommonApiEndpoints(factory);

	Log.Information("Динамический шлюз запущен и готов к эсплуатации.");
	
	services.AddScoped<QueuesRepository>();

	services.AddTransient<RabbitMqQueueListener>();
	services.AddSingleton<QueueListenerService>();

	// Удалить, если читаешь 3 раз:
	//using (var scope = app.Services.CreateScope())
	//{
	//	var queueListenerService = scope.ServiceProvider.GetRequiredService<QueueListenerService>();
	//	var consumers = await queueListenerService.StartQueueListenersAsync(cts.Token);

	//	app.Lifetime.ApplicationStopping.Register(() =>
	//	{
	//		foreach (var listener in consumers)
	//		{
	//			listener.StopListening();
	//		}
	//	});
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
