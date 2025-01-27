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

// ��������� �����������
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
		Log.Error("���� �� ������. ������: MyApp.exe --port=5001");
		return;
	}

	string url = $"http://localhost:{port}";
	builder.WebHost.UseUrls(url);
	Log.Information("���������� ����� �������� �� ������: {Url}", url);

	// ����������� ��������
	builder.Services.AddCoreServices();
	builder.Services.AddRabbitMqServices();
	builder.Services.AddApiServices();

	Log.Information("��� ������� ������� ����������������.");

	var app = builder.Build();

	// ������������� CORS
	app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
	Log.Information("CORS �������� ��� ���� ����������.");

	// ��������� ����������� ��������
	app.UseSerilogRequestLogging();
	Log.Information("����������� HTTP-�������� ��������.");

	// ����������� �������� �����
	app.MapCommonApiEndpoints();
	app.MapTcpApiEndpoints();
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
