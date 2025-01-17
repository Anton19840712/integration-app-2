using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using servers_api.Patterns;

namespace servers_api.endpoints
{
	public static class ApiEndpoints
	{
		public static void MapApiEndpoints(this IEndpointRouteBuilder app)
		{
			// GET-запрос для проверки доступности сервера
			app.MapGet("/api/servers/ping", () =>
			{
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
					var message = await uploadFileService.ConfigureAsync(jsonBody, stoppingToken);
					return Results.Ok();
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error during file upload");
					return Results.Problem(ex.Message);
				}
			});
		}
	}
}
