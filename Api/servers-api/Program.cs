using RabbitMQ.Client;
using Serilog;
using servers_api.endpoints;
using servers_api.Handlers;
using servers_api.Patterns;
using servers_api.Services.Brokers;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;
using tcp_client;

Console.Title = "Client API";

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.Host.UseSerilog((ctx, cfg) =>
{
	cfg.WriteTo.Console()
		.WriteTo.Seq("https://seq.pit.protei.ru/")
	   //.WriteTo.Seq("http://localhost:5341")
	   .Enrich.FromLogContext();
});

try
{
	string port = args.FirstOrDefault(arg => arg.StartsWith("--port="))?.Split('=')[1];
	if (string.IsNullOrEmpty(port))
	{
		Log.Error("Порт не указан. Пример: MyApp.exe --port=5001");
		return;
	}

	string url = $"http://localhost:{port}";
	builder.WebHost.UseUrls(url);
	Log.Information("Приложение будет запущено по адресу: {Url}", url);

	// Регистрация сервисов
	builder.Services.AddCoreServices();
	builder.Services.AddRabbitMqServices();
	builder.Services.AddApiServices();

	Log.Information("Все сервисы успешно зарегистрированы.");

	var app = builder.Build();

	// Использование CORS
	app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
	Log.Information("CORS настроен для всех источников.");

	// Включение логирования запросов
	app.UseSerilogRequestLogging();
	Log.Information("Логирование HTTP-запросов включено.");

	// Регистрация конечных точек
	app.MapCommonApiEndpoints();
	app.MapTcpApiEndpoints();
	Log.Information("Все конечные точки зарегистрированы.");

	// Запуск приложения
	Log.Information("Приложение запускается...");
	app.Run();
}
catch (Exception ex)
{
	// Логирование критических ошибок
	Log.Fatal(ex, "Критическая ошибка при запуске приложения");
	throw;
}
finally
{
	// Завершение работы логера
	Log.CloseAndFlush();
}

static class ServiceCollectionExtensions
{
	/// <summary>
	/// Регистрация базовых сервисов приложения
	/// </summary>
	public static IServiceCollection AddCoreServices(this IServiceCollection services)
	{
		Log.Information("Регистрация базовых сервисов...");
		services.AddHttpClient();
		services.AddHttpContextAccessor();
		services.AddCors();
		services.AddHostedService<ResponseListenerService>();
		Log.Information("Базовые сервисы зарегистрированы.");
		return services;
	}

	/// <summary>
	/// Регистрация RabbitMQ сервисов
	/// </summary>
	public static IServiceCollection AddRabbitMqServices(this IServiceCollection services)
	{
		Log.Information("Инициализация RabbitMQ...");
		services.AddSingleton<IConnectionFactory>(provider =>
		{
			// UNCOMMENT
			//
			var factory = new ConnectionFactory
			{

				HostName = "localhost",
				Port = 5672,
				UserName = "guest",
				Password = "guest"
			};
			//var factory = new ConnectionFactory
			//{
			//	Uri = new Uri("amqp://admin:admin@172.16.211.18/termidesk")
			//};

			Log.Information("RabbitMQ настроен: {Host}:{Port}", factory.HostName, factory.Port);
			return factory;
		});

		services.AddSingleton<IRabbitMqQueueListener, RabbitMqQueueListener>();
		services.AddSingleton<IRabbitMqService, RabbitMqService>();
		Log.Information("Сервисы RabbitMQ зарегистрированы.");
		return services;
	}

	/// <summary>
	/// Регистрация API сервисов
	/// </summary>
	public static IServiceCollection AddApiServices(this IServiceCollection services)
	{
		Log.Information("Регистрация API-сервисов...");
		services.AddTransient<IJsonParsingService, JsonParsingService>();
		services.AddTransient<IRabbitMqQueueManager, RabbitMqQueueManager>();
		services.AddTransient<ITeachService, TeachService>();
		services.AddScoped<IUploadService, UploadService>();
		services.AddTransient<IUploadHandler, UploadHandler>();
		services.AddScoped<ISenderService, SenderService>();
		Log.Information("API-сервисы зарегистрированы.");
		return services;
	}
}
