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
	/// <summary>
	/// Эндпоинт занимается отсылкой файла через рест запрос в очередь.
	/// </summary>
	/// <param name="file">Загружаемый файл.</param>
	/// <param name="queueName">Название очереди.</param>
	/// <returns></returns>
	[HttpPost("upload/{queueName}")]
	public async Task<IActionResult> UploadFile(IFormFile file, string queueName)
	{
		try
		{
			// Проверяем, что название очереди не пустое
			if (string.IsNullOrWhiteSpace(queueName))
			{
				return BadRequest("Название очереди не может быть пустым.");
			}

			// Копируем содержимое файла в память
			using var stream = new MemoryStream();
			await file.CopyToAsync(stream);
			byte[] fileContent = stream.ToArray();

			// Вычисляем хеш файла
			string fileHash = ComputeFileHash(fileContent);

			// Проверяем, был ли этот файл уже загружен
			if (!_fileHashService.TryAddHash(fileHash))
			{
				_logger.LogInformation("Файл уже был загружен: {FileName}", file.FileName);
				return BadRequest("Этот файл уже был загружен.");
			}

			// Получаем расширение файла
			string fileExtension = Path.GetExtension(file.FileName);

			// Создаем объект сообщения для отправки в очередь
			var message = new
			{
				FileName = file.FileName,           // Добавляем имя файла
				FileContent = Convert.ToBase64String(fileContent), // Кодируем в base64
			};

			string jsonMessage = JsonConvert.SerializeObject(message);
			await _rabbitMqService.PublishMessageAsync(queueName, queueName, jsonMessage);

			// Возвращаем успешный ответ
			return Ok($"Файл успешно загружен и передан в очередь '{queueName}'.");
		}
		catch (Exception ex)
		{
			// Логируем ошибку и возвращаем статус 500
			_logger.LogError(ex, "Ошибка при загрузке файла.");
			return StatusCode(500, "Произошла ошибка при обработке файла.");
		}
	}

	/// <summary>
	/// Вычисляет хеш файла с использованием алгоритма SHA256.
	/// </summary>
	/// <param name="fileContent">Содержимое файла в байтах.</param>
	/// <returns>Хеш файла в строковом представлении.</returns>
	private static string ComputeFileHash(byte[] fileContent)
	{
		using var sha256 = SHA256.Create();
		byte[] hashBytes = sha256.ComputeHash(fileContent);
		return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
	}
}