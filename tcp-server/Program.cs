using Serilog;

Console.Title = "tcp-server";

var builder = WebApplication.CreateBuilder(args);

// ��������� Serilog
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

builder.Host.UseSerilog();

// �������� ����������
if (args.Length == 0)
{
	Log.Fatal("������: ��������� ������� �� ��������. ��������� ������: <host>,<port>");
	return;
}

var parameters = args[0].Split(',');
if (parameters.Length < 2 || !int.TryParse(parameters[1], out var port))
{
	Log.Fatal("������: �������� ������ ����������. ��������� ������: <host>,<port>");
	return;
}

var host = parameters[0];

// ��������� ������ �������
string url = $"http://{host}:{port}";
builder.WebHost.UseUrls(url);

// ����������� ������ �������
Log.Information("���������� server ����������� �� {Url}", url);

var app = builder.Build();
app.Run();
