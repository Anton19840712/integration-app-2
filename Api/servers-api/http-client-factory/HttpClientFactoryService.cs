namespace servers_api.http_client_factory
{
	public class HttpClientFactoryService : IHttpClientFactoryService
	{
		public HttpClient CreateClient()
		{
			// Настройка HttpClient с базовым адресом и тайм-аутами
			var client = new HttpClient
			{
				BaseAddress = new Uri("http://localhost") // Пример базового URL, вы можете изменять
			};

			// Дополнительные настройки HttpClient, например, заголовки, тайм-ауты и т.д.
			client.Timeout = TimeSpan.FromSeconds(30);

			return client;
		}
	}
}
