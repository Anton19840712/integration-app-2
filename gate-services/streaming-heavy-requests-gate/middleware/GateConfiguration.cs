﻿using Newtonsoft.Json.Linq;

namespace rtsp_dynamic_gate_app.middleware;

/// <summary>
/// Класс конфигурации для RTSP шлюза.
/// Загружает настройки из конфигурационного файла и сохраняет их в Configuration.
/// </summary>
public class GateConfiguration
{
	public (string HttpUrl, string HttpsUrl) ConfigureRtspGate(string[] args, WebApplicationBuilder builder)
	{
		var configFilePath = args.FirstOrDefault(a => a.StartsWith("--config="))?.Substring(9) ?? "./configs/rtsp.json";
		var config = LoadConfiguration(configFilePath);

		var gateway = config["GatewaySettings"];
		var rtsp = config["RtspSettings"];

		if (gateway == null || rtsp == null)
			throw new InvalidOperationException("Конфигурация должна содержать GatewaySettings и RtspSettings.");

		// Настройки шлюза
		var companyName = gateway["CompanyName"]?.ToString() ?? "RtspCompany";
		var host = gateway["Host"]?.ToString() ?? "127.0.0.1";
		var port = int.TryParse(gateway["Port"]?.ToString(), out var p) ? p : 8554;

		builder.Configuration["CompanyName"] = companyName;
		builder.Configuration["Host"] = host;
		builder.Configuration["Port"] = port.ToString();

		// Добавляем секцию RtspSettings
		foreach (var prop in rtsp.Children<JProperty>())
		{
			var key = $"RtspSettings:{prop.Name}";

			if (prop.Value is JArray array)
			{
				for (int i = 0; i < array.Count; i++)
				{
					builder.Configuration[$"{key}:{i}"] = array[i]?.ToString();
				}
			}
			else
			{
				builder.Configuration[key] = prop.Value?.ToString();
			}
		}

		var httpUrl = $"http://{host}:80";
		var httpsUrl = $"https://{host}:443";

		return (httpUrl, httpsUrl);
	}

	private static JObject LoadConfiguration(string configFilePath)
	{
		try
		{
			string fullPath;

			if (Path.IsPathRooted(configFilePath))
			{
				fullPath = configFilePath;
			}
			else
			{
				var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
				fullPath = Path.GetFullPath(Path.Combine(basePath, configFilePath));
			}

			// Печатаем информацию для отладки.
			Console.WriteLine();
			Console.WriteLine($"[INFO] Конечный путь к конфигу: {fullPath}");
			Console.WriteLine($"[INFO] Загружается конфигурация: {Path.GetFileName(fullPath)}");
			Console.WriteLine();

			// Проверка существования файла.
			if (!File.Exists(fullPath))
				throw new FileNotFoundException("Файл конфигурации не найден", fullPath);

			// Загружаем конфигурацию из файла.
			var json = File.ReadAllText(fullPath);
			return JObject.Parse(json);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Ошибка при загрузке конфигурации из файла '{configFilePath}': {ex.Message}");
		}
	}
}
