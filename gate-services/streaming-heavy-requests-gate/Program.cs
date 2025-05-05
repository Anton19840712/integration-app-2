using rtsp_dynamic_gate_app.background;
using rtsp_dynamic_gate_app.middleware;
using rtsp_dynamic_gate_app.models;
using Serilog;

//��������������, ��� ���������� ����������� ��� ������������ ������� � ������� ��������. Highload.
//� ��� ����� ����� ��������� ��� ���������, ������� ����� ��������� ������� ���������� ������� rps.

Console.Title = "highload-rtsp-requests-gate";

var builder = WebApplication.CreateBuilder(args);

// �������� �����������
LoggingConfiguration.ConfigureLogging(builder);

// �������� ������������ � ��������� URL
var configLoader = new GateConfiguration();
var (httpUrl, httpsUrl) = configLoader.ConfigureRtspGate(args, builder);

// ������������ RTSP-�������
builder.Services.Configure<RtspSettings>(builder.Configuration.GetSection("RtspSettings"));
builder.Services.AddControllers();
builder.Services.AddSingleton<RtspStreamingService>(); // ������ � ������ ��� ���������
builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<RtspStreamingService>());

// ���������� CORS
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

try
{
	app.MapControllers();

	app.Urls.Add(httpUrl);
	app.Urls.Add(httpsUrl);

	Log.Information("Middleware: ������������ ���� ������� �� {HttpUrl} � {HttpsUrl}", httpUrl, httpsUrl);

	// ����������� �������� � Serilog
	app.UseSerilogRequestLogging();

	// �������� CORS � ���������
	app.UseCors(); // ���� ����� ������ ���� ����� UseRouting, �� �� UseAuthorization

	// ��������� ����������
	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "����������� ������ ��� ������� ����������");
}
finally
{
	Log.CloseAndFlush();
}
