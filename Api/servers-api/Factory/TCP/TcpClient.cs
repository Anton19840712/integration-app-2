using servers_api.Factory.Abstractions;

namespace servers_api.Factory.TCP
{
	    public class TcpClient : IClient
	    {
	        // тут мы пытаемся соединиться с их сервером
	        // для примера это пока будет наш сервер, с которым мы будем соединяться
	        // в перспективе для осознанного проектирования системы лучше иметь имитацию запущенного их сервера.
	        public void ConnectToServer(string host, int port)
	        {
	            // здесь нужно реализовать логику, как мы будем коннектиться будучи клиентом к их серверу, внешнему для нашего контура
	            Console.WriteLine($"TCP Client: Connecting to {host}:{port}");
	        }
	    }
}
