using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

namespace servers_api.api.controllers;

[Route("/api")]
public class TcpController(ILogger<TcpController> logger) : ControllerBase
{
	private readonly IModel _channel;
	private static readonly char[] trimChars = ['\uFEFF', '\u200B'];

	[HttpPost]
	[Route("message")]
	public async Task<IActionResult> Message()
	{
		using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
		var requestBodyAsString = await reader.ReadToEndAsync();

		logger.LogInformation("Received SOAP request: {Request}", requestBodyAsString);

		// Преобразование XML в JSON
		string json = ConvertXmlToJson(requestBodyAsString);
		logger.LogInformation("Converted JSON: {Json}", json);

		// Отправка JSON в очередь RabbitMQ
		PublishMessageToQueue(json);

		// Отправка SOAP ответа
		string payload = "<?xml version='1.0' encoding='utf-8'?>\r\n" +
						 "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
						 "<soapenv:Body>\r\n" +
						 "<card112ChangedResponse xmlns=\"http://www.protei.ru/emergency/integration\">\r\n" +
						 "<errorCode>0</errorCode>\r\n" +
						 "<errorMessage></errorMessage>\r\n" +
						 "</card112ChangedResponse>\r\n" +
						 "</soapenv:Body></soapenv:Envelope>";

		await HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(payload));
		logger.LogInformation("Sent SOAP response: {Response}", payload);

		return new EmptyResult();
	}

	private void PublishMessageToQueue(string jsonMessage)
	{
		var body = Encoding.UTF8.GetBytes(jsonMessage);

		// Публикация сообщения в RabbitMQ
		_channel.BasicPublish(exchange: "",
							 routingKey: "bpmn_queue",
							 basicProperties: null,
							 body: body);

		logger.LogInformation("Message published to RabbitMQ bpmn_queue: {Message}", jsonMessage);
	}

	private static string ConvertXmlToJson(string xml)
	{
		var xmlDoc = new XmlDocument();
		xml = xml.TrimStart(trimChars);
		xml = xml[xml.IndexOf('<')..];

		xmlDoc.LoadXml(xml);

		XmlNode bodyNode = xmlDoc.SelectSingleNode("//*[local-name()='Body']");

		if (bodyNode != null)
		{
			var jsonSettings = new JsonSerializerSettings
			{
				Formatting = Newtonsoft.Json.Formatting.Indented,
				Converters = { new Newtonsoft.Json.Converters.XmlNodeConverter { OmitRootObject = true } }
			};

			string jsonText = JsonConvert.SerializeObject(bodyNode["card112ChangedRequest"], jsonSettings);

			var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonText);
			jsonObject.Descendants().OfType<JProperty>()
					  .Where(attr => attr.Name.StartsWith('@'))
					  .ToList()
					  .ForEach(attr => attr.Remove());

			return JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
		}

		return "{}";
	}
}
