using Serilog;
using sftp_dynamic_gate_app.middleware;
using sftp_dynamic_gate_app.models;

Console.Title = "batch-processing-requests-gate";

var builder = WebApplication.CreateBuilder(args);

LoggingConfiguration.ConfigureLogging(builder);

ConfigureServices(builder);

var app = builder.Build();

try
{
	// ��������� ������������� ����� (����� ������������������ ������)
	var gateConfigurator = app.Services.GetRequiredService<GateConfiguration>();
	var (httpUrl, httpsUrl) = gateConfigurator.ConfigureSftpGate(args, builder);

	// ��������� ��������� ����������
	ConfigureApp(app, httpUrl, httpsUrl);

	// ���������
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
	var configuration = builder.Configuration;
	var services = builder.Services;

	services.AddSingleton<GateConfiguration>();
	services.AddRabbitMqServices(configuration);
	services.AddSftpServices(configuration);

	services.AddControllers();
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
