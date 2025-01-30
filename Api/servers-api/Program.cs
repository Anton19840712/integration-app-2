using Serilog;
using servers_api.middleware;
using servers_api.rest.minimalapi;

Console.Title = "integration api";

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
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

	Log.Information("��� ������� ������� ����������������.");

	var app = builder.Build();

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
