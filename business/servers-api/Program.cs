using Microsoft.Extensions.Options;
using MongoDB.Driver;
using rabbit_listener;
using RabbitMQ.Client;
using Serilog;
using servers_api.api.controllers;
using servers_api.api.minimal;
using servers_api.middleware;
using servers_api.models.configurationsettings;
using servers_api.models.entities;
using servers_api.models.outbox;
using servers_api.repositories;
using servers_api.services.brokers.bpmintegration;

Console.Title = "integration api";

var builder = WebApplication.CreateBuilder(args);
var cts = new CancellationTokenSource();

// ��������� �����������
builder.Host.UseSerilog((ctx, cfg) =>
{
cfg.MinimumLevel.Information()
   .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
   .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
   .Enrich.FromLogContext();
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

	//-------------

	services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

	// ������ ������������ ��� ������������:
	builder.Services.AddTransient<IMongoRepository<QueuesEntity>, MongoRepository<QueuesEntity>>();
	builder.Services.AddTransient<IMongoRepository<OutboxMessage>, MongoRepository<OutboxMessage>>();
	builder.Services.AddTransient<IMongoRepository<IncidentEntity>, MongoRepository<IncidentEntity>>();

	services.AddSingleton<MongoRepository<OutboxMessage>>();
	services.AddSingleton<MongoRepository<QueuesEntity>>();
	services.AddSingleton<MongoRepository<IncidentEntity>>();

	services.AddHostedService<QueueListenerBackgroundService>();

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

	services.AddTransient<FileHashService>();

	services.AddSingleton<SftpConfig>(new SftpConfig
	{
		Host = "192.168.100.3",
		Port = 22,
		UserName = "tester",
		Password = "password"
	});
	services.AddSingleton<IConnectionFactory, ConnectionFactory>(_ =>
		new ConnectionFactory { HostName = "localhost" });
	services.AddTransient<IRabbitMqQueueListener, RabbitMqSftpListener>();
	services.AddTransient<IRabbitMqQueueListener, RabbitMqQueueListener>();

	//-------------

	var app = builder.Build();

	app.UseSerilogRequestLogging();
	app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

	var factory = app.Services.GetRequiredService<ILoggerFactory>();

	app.MapControllers();
	app.MapIntegrationMinimalApi(factory);
	app.MapAdminMinimalApi(factory);
	app.MapTestMinimalApi(factory);

	Log.Information("������������ ���� ������� � ����� � ������������.");	

	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "����������� ������ ��� ������� ����������");
	throw;
}
finally
{
	cts.Cancel();
	cts.Dispose();
	Log.CloseAndFlush();
}
