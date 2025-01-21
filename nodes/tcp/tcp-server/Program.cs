using Serilog;

Console.Title = "tcp-server";

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

builder.Host.UseSerilog();

// Проверка аргументов
if (args.Length == 0)
{
	Log.Fatal("Ошибка: Параметры запуска не переданы. Ожидается формат: <host>,<port>");
	return;
}

var parameters = args[0].Split(',');
if (parameters.Length < 2 || !int.TryParse(parameters[1], out var port))
{
	Log.Fatal("Ошибка: Неверный формат параметров. Ожидается формат: <host>,<port>");
	return;
}

var host = parameters[0];

// Настройка адреса запуска
string url = $"http://{host}:{port}";
builder.WebHost.UseUrls(url);

// Логирование адреса запуска
Log.Information("Приложение server запускается на {Url}", url);

var app = builder.Build();
app.Run();
