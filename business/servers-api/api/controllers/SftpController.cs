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
	/// �������� ���������� �������� ����� ����� ���� ������ � �������.
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
				return BadRequest("�������� ������� �� ����� ���� ������.");
			}

			using var stream = new MemoryStream();
			await file.CopyToAsync(stream);
			byte[] fileContent = stream.ToArray();

			string fileHash = ComputeFileHash(fileContent);

			if (!_fileHashService.TryAddHash(fileHash))
			{
				_logger.LogInformation("���� ��� ��� ��������: {FileName}", file.FileName);
				return BadRequest("���� ���� ��� ��� ��������.");
			}

			string fileExtension = Path.GetExtension(file.FileName);

			// ���������� ���� � ������� ����� RabbitMqService
			var message = new
			{
				FileContent = Convert.ToBase64String(fileContent), // �������� � base64, ����� �������� � JSON
				FileExtension = fileExtension
			};

			string jsonMessage = JsonConvert.SerializeObject(message);
			await _rabbitMqService.PublishMessageAsync(queueName, queueName, jsonMessage);

			return Ok($"���� ������� �������� � ������� � ������� '{queueName}'.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "������ ��� �������� �����.");
			return StatusCode(500, "��������� ������ ��� ��������� �����.");
		}
	}

	private static string ComputeFileHash(byte[] fileContent)
	{
		using var sha256 = SHA256.Create();
		byte[] hashBytes = sha256.ComputeHash(fileContent);
		return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
	}
}