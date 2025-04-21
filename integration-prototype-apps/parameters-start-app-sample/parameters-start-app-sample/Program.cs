using servers_api.validation.headers;

public class Program
{
	public static void Main(string[] args)
	{
		// ��������� ��������� �� launchSettings.json � environment variables
		var builder = WebApplication.CreateBuilder(args);

		// �������� ��������� �� ��������� ������
		var port = GetArgument(args, "--port=", GetConfigValue(builder, "applicationUrl", 5000));
		var host = GetArgument(args, "--host=", "localhost");
		var enableValidation = GetArgument(args, "--validate=", false);

		// ��������� �������
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddAuthorization();
		builder.Services.AddControllers();

		// ���� ��������� ��������, ������������ ������ ���������
		//var enableValidation1 = true;
		if (enableValidation)
		{
			builder.Services.AddScoped<SimpleHeadersValidator>();
			builder.Services.AddScoped<DetailedHeadersValidator>();
			Console.WriteLine("��������� ��������");
		}
		else
		{
			Console.WriteLine("��������� ���������");
		}

		var app = builder.Build();

		app.UseAuthorization();
		app.MapControllers();

		// ������������� URL � ����������� �����������
		var url = $"http://{host}:{port}";
		app.Urls.Add(url);

		Console.WriteLine($"������ ������� �� {url}");

		app.Run();
	}

	// ����� ��� �������� ���������� ��������� ������
	private static T GetArgument<T>(string[] args, string key, T defaultValue)
	{
		var arg = args.FirstOrDefault(a => a.StartsWith(key));
		if (arg != null)
		{
			var value = arg.Substring(key.Length);
			try
			{
				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch
			{
				Console.WriteLine($"������ ��� ������� ��������� {key}. ������������ �������� �� ���������: {defaultValue}");
			}
		}
		return defaultValue;
	}

	// ����� ��� ��������� �������� �� ������������ (��������, �� launchSettings.json)
	private static T GetConfigValue<T>(WebApplicationBuilder builder, string key, T defaultValue)
	{
		var configValue = builder.Configuration[key];
		if (configValue != null)
		{
			try
			{
				return (T)Convert.ChangeType(configValue, typeof(T));
			}
			catch
			{
				Console.WriteLine($"������ ��� ������� ������������ �� ����� {key}. ������������ �������� �� ���������: {defaultValue}");
			}
		}
		return defaultValue;
	}
}
