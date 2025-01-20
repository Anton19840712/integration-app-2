using servers_api.Factory.TCP;
using servers_api.Factory.UDP;

namespace servers_api.Factory.Abstractions
{
	/// <summary>
	/// Класс, который поднимает в динамическом шлюзе
	/// согласно входящей информации либо клиент либо сервер определенного соединения.
	/// </summary>
	public class ProtocolManager
	    {
		public void Configure(
	            string protocol,
	            bool isServer,
	            string address = null,
	            string host = null,
	            int? port = 0)
	        {
	            ProtocolFactory factory = protocol switch
	            {
	                // тут выбирается, по какому типу соединения будет происходить либо поднятие сервера, либо клиента
	                "TCP" => new TcpFactory(),
	                "UDP" => new UdpFactory(),
	                _ => throw new ArgumentException("Unsupported protocol")
	            };

	            // если администратор заказал поднять сервер в конфигурационном файле:
	            if (isServer)
	            {
	                var server = factory.CreateServer();

	                server.UpServer(host, port);

	                server.SendServerAddress(host, port);
	            }
	            else
	            {
	                if (port.HasValue) // Если port не равен null
	                {
	                    // создаем определенный клиент согласно типу протокола и им соединяемся к их серверу
	                    IClient client = factory.CreateClient();
	                    client.ConnectToServer(host, port.Value); // Доступ к значению port
	                }
	                else
	                {
	                    // Логика на случай, если порт равен null (если нужно)
	                    // Например, можно использовать дефолтный порт:
	                    IClient client = factory.CreateClient();
	                    client.ConnectToServer(host, 5000); // Пример с дефолтным портом
	                }
	            }
	        }
	    }
}
