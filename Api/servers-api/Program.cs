using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using servers_api.factory.abstractions;
using servers_api.factory.tcp.instancehandlers;
using servers_api.factory.tcp.instances;
using servers_api.factory.tcp.queuesconnections;
using servers_api.Handlers;
using servers_api.models.configurationsettings;
using servers_api.Patterns;
using servers_api.rest.minimalapi;
using servers_api.services.brokers.bpmintegration;
using servers_api.services.brokers.tcprest;
using servers_api.Services.Connectors;
using servers_api.Services.InternalSystems;
using servers_api.Services.Parsers;

Console.Title = "Client API";

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Host.UseSerilog((ctx, cfg) =>
{
	cfg
	.WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
		//.WriteTo.Seq("https://seq.pit.protei.ru/")
		//.WriteTo.Seq("http://localhost:5341")
		.Enrich.FromLogContext()
		;
});

try
{
	//string port = args.FirstOrDefault(arg => arg.StartsWith("--port="))?.Split('=')[1];
	//if (string.IsNullOrEmpty(port))
	//{
	//	Log.Error("���� �� ������. ������: MyApp.exe --port=5001");
	//	return;
	//}

	//string url = $"http://localhost:{port}";
	//builder.WebHost.UseUrls(url);
	//Log.Information("���������� ����� �������� �� ������: {Url}", url);

	// ����������� ��������

	var services = builder.Services;
	var configuration = builder.Configuration;

	services.AddScoped<ProtocolManager>(); // ������������ ProtocolManager
	services.AddControllers();
	services.AddCoreServices();
	services.AddRabbitMqServices();
	services.AddApiServices();

	// ������������ TcpServer � TcpClient ��� �������
	services.AddScoped<TcpServer>();
	services.AddScoped<TcpClient>();

	// ������������ TcpFactory
	services.AddScoped<TcpFactory>();

	// ����������� ������������ ������:
	services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

	// ����� ����� ����������� ��������:
	services.AddScoped<ITcpServerHandler, TcpServerHandler>();

	Log.Information("��� ������� ������� ����������������.");


	var app = builder.Build();

	// ����������� ��������
	app.UseSerilogRequestLogging();

	// ������������� CORS
	app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
	Log.Information("CORS �������� ��� ���� ����������.");

	// ��������� ����������� ��������
	app.UseSerilogRequestLogging();
	Log.Information("����������� HTTP-�������� ��������.");


	var factory = app.Services.GetRequiredService<ILoggerFactory>();
	// ����������� �������� �����
	app.MapControllers();
	app.MapCommonApiEndpoints(factory);
	Log.Information("��� �������� ����� ����������������.");

	// ������ ����������
	Log.Information("���������� �����������...");
	app.Run();
}
catch (Exception ex)
{
	// ����������� ����������� ������
	Log.Fatal(ex, "����������� ������ ��� ������� ����������");
	throw;
}
finally
{
	// ���������� ������ ������
	Log.CloseAndFlush();
}

static class ServiceCollectionExtensions
{
	/// <summary>
	/// ����������� ������� �������� ����������
	/// </summary>
	public static IServiceCollection AddCoreServices(this IServiceCollection services)
	{
		Log.Information("����������� ������� ��������...");
		services.AddHttpClient();
		services.AddHttpContextAccessor();
		services.AddCors();
		services.AddHostedService<ResponseListenerService>();

		services.AddTransient<TcpFactory>();

		Log.Information("������� ������� ����������������.");
		return services;
	}

	/// <summary>
	/// ����������� RabbitMQ ��������
	/// </summary>
	public static IServiceCollection AddRabbitMqServices(this IServiceCollection services)
	{
		Log.Information("������������� RabbitMQ...");
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

			Log.Information("RabbitMQ ��������: {Host}:{Port}", factory.HostName, factory.Port);
			return factory;
		});

		services.AddSingleton<IRabbitMqQueueListener, RabbitMqQueueListener>();
		services.AddSingleton<IRabbitMqService, RabbitMqService>();
		Log.Information("������� RabbitMQ ����������������.");
		return services;
	}

	/// <summary>
	/// ����������� API ��������
	/// </summary>
	public static IServiceCollection AddApiServices(this IServiceCollection services)
	{
		Log.Information("����������� API-��������...");
		services.AddTransient<IJsonParsingService, JsonParsingService>();
		services.AddTransient<IRabbitMqQueueManager, RabbitMqQueueManager>();
		services.AddTransient<ITeachService, TeachService>();
		services.AddScoped<IUploadService, UploadService>();
		services.AddTransient<IUploadHandler, UploadHandler>();
		services.AddScoped<ISenderService, SenderService>();
		Log.Information("API-������� ����������������.");
		return services;
	}
}
