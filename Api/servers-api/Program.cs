using Serilog;
using servers_api.middleware;
using servers_api.rest.minimalapi;

Console.Title = "integration api";

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.Host.UseSerilog((ctx, cfg) =>
{
	cfg.WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
	.Enrich.FromLogContext();
});

try
{
	//GateConfiguration.ConfigureDynamicGate(args, builder);

	var services = builder.Services;
	var configuration = builder.Configuration;

	services.AddControllers();

	services.AddCommonServices();
	services.AddHttoServices();
	services.AddFactoryServices();
	services.AddApiServices();
	services.AddRabbitMqServices();

	services.AddAutoMapper(typeof(MappingProfile));

	Log.Information("Все сервисы успешно зарегистрированы.");

	var app = builder.Build();

	app.UseSerilogRequestLogging();

	// Использование CORS
	app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
	Log.Information("CORS настроен для всех источников.");

	// Включение логирования запросов
	app.UseSerilogRequestLogging();
	Log.Information("Логирование HTTP-запросов включено.");

	var factory = app.Services.GetRequiredService<ILoggerFactory>();

	// Регистрация конечных точек
	app.MapControllers();
	app.MapCommonApiEndpoints(factory);
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
