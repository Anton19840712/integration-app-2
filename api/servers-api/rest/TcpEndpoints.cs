namespace servers_api.rest
{
	public static class TcpEndpoints
	{
		public static void MapTcpApiEndpoints(this IEndpointRouteBuilder app, ILoggerFactory loggerFactory)
		{
			var logger = loggerFactory.CreateLogger("TcpEndpoints");
			// Здесь я отправляю определенное сообщение, полученное из post запроса в очередь:
			app.MapPost("/send-request", async (HttpRequest request, IRabbitMqService rabbitMqService) =>
			{
				// Получение тела запроса:
				using var reader = new StreamReader(request.Body);
				var message = await reader.ReadToEndAsync();
				logger.LogInformation("Запрос отправлен: {Message}", message);

				// Отправка сообщения
				rabbitMqService.PublishMessage("request_queue", message);

				// Ожидание ответа
				var responseMessage = await rabbitMqService.WaitForResponse("response_queue");
				if (responseMessage != null)
				{
					logger.LogInformation($"Получен ответ: {responseMessage}");
					return Results.Ok(new { Message = responseMessage });
				}

				logger.LogWarning("Тайм-аут при ожидании ответа");
				return Results.StatusCode(504);
			});
		}
	}
}
