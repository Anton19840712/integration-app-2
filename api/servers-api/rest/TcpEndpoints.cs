using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using servers_api.Patterns;

namespace servers_api.rest
{
	public static class TcpEndpoints
	{
		public static void MapTcpApiEndpoints(this IEndpointRouteBuilder app)
		{
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
		}
	}
}
