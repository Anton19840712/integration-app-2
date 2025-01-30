using System.Text.Json;
using Newtonsoft.Json;
using System.Xml;
using servers_api.Services.Parsers;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonException = System.Text.Json.JsonException;
using servers_api.models.internallayerusage;
using servers_api.models.configurationsettings;
public class JsonParsingService : IJsonParsingService
{
	private readonly ILogger<JsonParsingService> _logger;

	public JsonParsingService(ILogger<JsonParsingService> logger)
	{
		_logger = logger;
	}
	/// <summary>
	/// Реализация парсера входящих моделей.
	/// Внутренняя модель внутри входящей должна быть передана в teach service, который будет ее отсылать в bpm.
	/// Сервис работает с json и xml форматами данных.
	/// </summary>
	/// <param name="jsonBody"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ApplicationException"></exception>
	public CombinedModel ParseJson(JsonElement jsonBody)
	{
		_logger.LogInformation("Начало разбора JSON");
		string jsonString = null;
		try
		{
			if (!jsonBody.TryGetProperty("protocol", out var protocolElement) ||
				!jsonBody.TryGetProperty("dataFormat", out var formatData) ||
				!jsonBody.TryGetProperty("companyName", out var companyNameElement) ||
				!jsonBody.TryGetProperty("model", out var modelElement) ||
				!jsonBody.TryGetProperty("dataOptions", out var dataOptionsElement) ||
				!jsonBody.TryGetProperty("connectionSettings", out var connectionSettingsElement))
			{
				_logger.LogWarning("Пропущены необходимые поля JSON");
				throw new ArgumentException("Пропущены необходимые поля JSON");
			}

			var protocol = protocolElement.GetString();
			var dataFormat = formatData.GetString();
			var companyName = companyNameElement.GetString();
			var inQueueName = $"{companyName}_in";
			var outQueueName = $"{companyName}_out";

			// Преобразуем JsonElement в строку без лишних переносов строк и пробелов
			jsonString = JsonSerializer.Serialize(modelElement);

			_logger.LogInformation("Будут созданы очереди: {InQueueName}, {OutQueueName}", inQueueName, outQueueName);

			// Десериализация dataOptions
			var dataOptions = JsonSerializer.Deserialize<DataOptions>(dataOptionsElement.GetRawText());
			_logger.LogInformation("Свойство dataOptions успешно десериализовано");

			// Десериализация connectionSettings с учетом наследования
			var connectionSettings = JsonSerializer.Deserialize<ConnectionSettings>(connectionSettingsElement.GetRawText());
			_logger.LogInformation("Свойство connectionSettings успешно десериализовано");

			// Обработка model в зависимости от dataFormat
			if (dataFormat == "xml")
			{
				// Парсим XML и конвертируем в JSON, убираем метаинформацию
				var xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(modelElement.GetString());

				// Убираем декларацию XML
				xmlDocument.RemoveChild(xmlDocument.FirstChild);

				// Конвертируем XML в JSON-строку
				jsonString = JsonConvert.SerializeXmlNode(xmlDocument, Newtonsoft.Json.Formatting.None, true);

				// Десериализуем JSON-строку в JsonElement
				_logger.LogInformation("XML успешно конвертирован в JSON");
			}
			else
			{
				// Если dataFormat не XML, считаем, что model уже JSON
				_logger.LogInformation("Модель в JSON формате успешно загружена");
			}

			var combinedModel = new CombinedModel
			{
				Protocol = protocol,
				InQueueName = inQueueName,
				OutQueueName = outQueueName,
				InternalModel = jsonString,
				DataOptions = dataOptions,
				ConnectionSettings = connectionSettings
			};

			_logger.LogInformation("JSON успешно разобран и преобразован в CombinedModel");
			return combinedModel;
		}
		catch (JsonException ex)
		{
			_logger.LogError(ex, "Ошибка при обработке JSON: неверный формат данных");
			throw new ArgumentException("Ошибка при обработке JSON: неверный формат данных.", ex);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Произошла ошибка при разборе JSON");
			throw new ApplicationException("Произошла ошибка при разборе JSON.", ex);
		}
	}
}