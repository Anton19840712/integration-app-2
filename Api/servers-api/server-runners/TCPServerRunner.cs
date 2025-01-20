using System.Diagnostics;

namespace servers_api.start;

public class TCPServerRunner : ITCPServerRunner
{
	public void RunTcpServer(string host, int? port)
	{
		if (!port.HasValue || port < 1 || port > 65535)
		{
			throw new ArgumentException($"Указан некорректный порт: {port}. Порт должен быть числом от 1 до 65535.");
		}
		try
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = @"D:\protei gateway\server-manager\tcp-server\bin\Debug\net8.0\tcp-server.exe",
				Arguments = $"{host},{port}", // Передаем параметры через запятую
				UseShellExecute = true,
				CreateNoWindow = false
			};

			Process.Start(processStartInfo);

		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка запуска приложения: {ex.Message}");
		}
	}
}
