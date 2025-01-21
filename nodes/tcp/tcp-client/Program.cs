using Serilog;
using tcp_client;

Console.Title = "client";

//этот код публикует данные в сетевую шину
//в какой ситуации это твой кейс?
//тогда, когда ты сервер и ты публикуешь данные в очередь,
//перед этим ты их получил из внешнего сервера, получается, ты для него клиент
//тогда получается, если ты клиент, то ты просто создаешь соединение tcp, а сам сервер, на котором создается тсп клиент
//будет выступать в качестве конфигурируемого нода.

// что может быть сконфигурировано, если я буду клиентом в нашем контуре, который должен будет

// 1 - принимать сообщение от внешнего tcp servera
// 2 - соединяться с очередью сетевой шины
// 3 - публиковать в очередь сетевой шины сообщения, полученного из внешнего тсп сервера
// 4 - получает запрошенные данные из очереди из out очереди сетевой шины, в которую опубликовала из своей базы система за данной сетевой шиной 
// получив информацию, клиент логирует полученное сообщение.
// 5 - сам сервер, на котором запущен наш tcp клиент должен иметь возможность конфигурироваться с точки зрения хоста, порта,
// названия очереди, в которую он должен подключаться и название очереди, из которой он будет читать, также времени на ожидание ответа из out очереди,
// так же периодичность попыток заслушивания из out очереди ответа.
// при создании json можно разбить на настройки самого сервера, на котором запущен клиент и на вложение, описывающее само поведение сервера при взаимодействии с сетевой шиной
// а именно: название очереди in, название очереди out, время на ожидание ответа из out очереди, отсрочка между процессом заслушивания сообщений.



var builder = WebApplication.CreateBuilder(args);

// Регистрация ResponseListenerService как фонового сервиса
builder.Services.AddHostedService<ResponseListenerService>();

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

builder.Host.UseSerilog();

// Чтение порта для запуска client
string? port = builder.Configuration["Port"]
			   ?? args.FirstOrDefault(arg => arg.StartsWith("--port="))?.Split('=')[1];

if (string.IsNullOrEmpty(port))
{
	port = "5001"; // Порт по умолчанию
}

// Настройка адреса запуска
string url = $"http://localhost:{port}";
builder.WebHost.UseUrls(url);

// Логирование адреса запуска
Log.Information("Приложение client запускается на {Url}", url);

builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

var app = builder.Build();

// Настройка маршрутов:
// Здесь я отправляю определенное сообщение, полученное из post запроса в очередь:
app.MapPost("/send-request", async (HttpRequest request, IRabbitMqService rabbitMqService) =>
{
	// Получение тела запроса:
	using var reader = new StreamReader(request.Body);
	var message = await reader.ReadToEndAsync();
	Log.Information("Запрос отправлен: {Message}", message);

	// Отправка сообщения
	rabbitMqService.PublishMessage("request_queue", message);

	// Ожидание ответа
	var responseMessage = await rabbitMqService.WaitForResponse("response_queue");
	if (responseMessage != null)
	{
		Log.Information($"Получен ответ: {responseMessage}");
		return Results.Ok(new { Message = responseMessage });
	}

	Log.Warning("Тайм-аут при ожидании ответа");
	return Results.StatusCode(504);
});

app.Run();
