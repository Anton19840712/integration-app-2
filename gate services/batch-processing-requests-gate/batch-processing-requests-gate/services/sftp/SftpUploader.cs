using Microsoft.Extensions.Options;
using Renci.SshNet;
using sftp_dynamic_gate_app.models;
using sftp_dynamic_gate_app.services.sftp;

public class SftpUploader : ISftpUploader, IDisposable
{
	private readonly SftpClient _client;
	private readonly ILogger<SftpUploader> _logger;
	private readonly string _remotePath;
	private readonly IConfiguration _configuration;

	public SftpUploader(IOptions<SftpSettings> options, ILogger<SftpUploader> logger, IConfiguration configuration)
	{
		var settings = options.Value;
		_logger = logger;
		_remotePath = settings.Source ?? ".";
		_configuration = configuration;
		_client = new SftpClient(settings.Host, settings.Port, settings.UserName, settings.Password);
		_client.Connect();
	}

	public async Task UploadAsync(string localFilePath, string remoteFileName = null)
	{
		var settings = new SftpSettings
		{
			Host = "172.16.205.118",
			Port = 22,
			UserName = "tester",
			Password = "password",
			Source = "." // Папка на SFTP
		};

		try
		{
			using var client = new SftpClient(settings.Host, settings.Port, settings.UserName, settings.Password);
			client.Connect();

			if (!client.IsConnected)
				throw new Exception("Не удалось подключиться к SFTP");

			string fileName = remoteFileName ?? Path.GetFileName(localFilePath);
			string remoteFullPath = settings.Source.EndsWith("/")
				? settings.Source + fileName
				: settings.Source + "/" + fileName;

			await using var fileStream = File.OpenRead(localFilePath);
			client.UploadFile(fileStream, remoteFullPath);

			_logger.LogInformation("Файл '{FileName}' успешно загружен в SFTP в папку '{RemotePath}'", fileName, settings.Source);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при загрузке файла на SFTP");
			throw;
		}
	}



	public void Dispose()
	{
		if (_client.IsConnected)
			_client.Disconnect();

		_client.Dispose();
	}
}
