using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using servers_api.Patterns;

namespace servers_api.rest
{
	public static class ApiEndpoints
	{
		public static void MapCommonApiEndpoints(this IEndpointRouteBuilder app, ILoggerFactory loggerFactory)
		{
			var logger = loggerFactory.CreateLogger("ApiEndpoints");

			// GET-запрос для проверки доступности сервера
			app.MapGet("/api/servers/ping", () =>
			{
				logger.LogInformation("Ping endpoint called");
				return Results.Ok(new { message = "Ping successful" });
			});

			// POST-запрос для загрузки файла
			app.MapPost("/api/servers/upload", async (
				[FromBody] JsonElement jsonBody,
				IUploadService uploadFileService,
				CancellationToken stoppingToken) =>
			{
				try
				{
					logger.LogInformation("Upload endpoint called with body: {JsonBody}", jsonBody.ToString());
					var result = await uploadFileService.ConfigureAsync(jsonBody, stoppingToken);
					logger.LogInformation("File uploaded successfully");
					return Results.Ok(result);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error during file upload");
					return Results.Problem(ex.Message);
				}
			});
		}
	}
}
