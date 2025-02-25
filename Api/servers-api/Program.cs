using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using servers_api.api.minimalapi;
using servers_api.middleware;
using servers_api.models.configurationsettings;
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

	services.AddSingleton<QueueListenerService>();

	services.AddTransient<IRabbitMqQueueListener, RabbitMqQueueListener>();

	services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

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
	app.MapCommonApiEndpoints(factory);

	Log.Information("Динамический шлюз запущен и готов к эсплуатации.");

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
