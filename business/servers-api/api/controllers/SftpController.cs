using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using servers_api.api.controllers;
using System.Security.Cryptography;

public class SftpController : ControllerBase
{
	private readonly IRabbitMqService _rabbitMqService;
	private readonly ILogger<SftpController> _logger;
	private readonly FileHashService _fileHashService;

	public SftpController(
		IRabbitMqService rabbitMqService,
		ILogger<SftpController> logger,
		FileHashService fileHashService)
	{
		_rabbitMqService = rabbitMqService;
		_logger = logger;
		_fileHashService = fileHashService;
	}

	/// <summary>
	/// Эндпоинт занимается отсылкой файла через рест запрос в очередь.
	/// </summary>
	/// <param name="file"></param>
	/// <param name="queueName"></param>
	/// <returns></returns>
	[HttpPost("upload/{queueName}")]
	public async Task<IActionResult> UploadFile(IFormFile file, string queueName)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(queueName))
			{
				return BadRequest("Название очереди не может быть пустым.");
			}

			using var stream = new MemoryStream();
			await file.CopyToAsync(stream);
			byte[] fileContent = stream.ToArray();

			string fileHash = ComputeFileHash(fileContent);

			if (!_fileHashService.TryAddHash(fileHash))
			{
				_logger.LogInformation("Файл уже был загружен: {FileName}", file.FileName);
				return BadRequest("Этот файл уже был загружен.");
			}

			string fileExtension = Path.GetExtension(file.FileName);

			// Отправляем файл в очередь через RabbitMqService
			var message = new
			{
				FileContent = Convert.ToBase64String(fileContent), // Кодируем в base64, чтобы передать в JSON
				FileExtension = fileExtension
			};

			string jsonMessage = JsonConvert.SerializeObject(message);
			await _rabbitMqService.PublishMessageAsync(queueName, queueName, jsonMessage);

			return Ok($"Файл успешно загружен и передан в очередь '{queueName}'.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при загрузке файла.");
			return StatusCode(500, "Произошла ошибка при обработке файла.");
		}
	}

	private static string ComputeFileHash(byte[] fileContent)
	{
		using var sha256 = SHA256.Create();
		byte[] hashBytes = sha256.ComputeHash(fileContent);
		return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
	}
}