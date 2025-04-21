using Serilog;
using servers_api.middleware;
using servers_api.services.senders;
using servers_api.services.teaching;

Console.Title = "integration api";

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

try
{
	var gateConfigurator = app.Services.GetRequiredService<GateConfiguration>();
	var (httpUrl, httpsUrl) = await gateConfigurator.ConfigureDynamicGateAsync(args, builder);

	//��������� ����������
	using var scope = app.Services.CreateScope();
	var teachIntegrationService = scope.ServiceProvider.GetRequiredService<ITeachIntegrationService>();
	var results = await teachIntegrationService.TeachAsync(CancellationToken.None);

	foreach (var item in results)
	{
		Log.Information($"������������ ����: {item.Message}");
	}

	ConfigureApp(app, httpUrl, httpsUrl);
	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "����������� ������ ��� ������� ����������");
	throw;
}
finally
{
	Log.CloseAndFlush();
}

static void ConfigureServices(WebApplicationBuilder builder)
{
	LoggingConfiguration.ConfigureLogging(builder);

	var configuration = builder.Configuration;
	var services = builder.Services;

	services.AddTransient<IQueueListenerService, QueueListenerService>();
	services.AddTransient<ITeachIntegrationService, TeachIntegrationService>();


	// ������������ GateConfiguration
	services.AddSingleton<GateConfiguration>();
	services.AddHttpClient();
	services.AddControllers();
	services.AddTransient<ITeachSenderHandler, TeachSenderHandler>();
	services.AddCommonServices();
	services.AddApiServices();
	services.AddRabbitMqServices(configuration);
	services.AddMongoDbServices(configuration);
	services.AddMongoDbRepositoriesServices(configuration);
	services.AddHostedServices();
}

static void ConfigureApp(WebApplication app, string httpUrl, string httpsUrl)
{
	app.Urls.Add(httpUrl);
	app.Urls.Add(httpsUrl);
	Log.Information($"Middleware: ������������ ���� ������� � ��������� ������� �� ��������� ������: {httpUrl} � {httpsUrl}");

	app.UseSerilogRequestLogging();

	app.UseCors(cors => cors
		.AllowAnyOrigin()
		.AllowAnyMethod()
		.AllowAnyHeader());

	app.MapControllers();
}
