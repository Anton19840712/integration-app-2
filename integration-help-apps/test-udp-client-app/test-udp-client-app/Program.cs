using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleApp1
{
	class Program
	{
		static UdpClient udp = new UdpClient();

		static void Main(string[] args)
		{
			Console.Title = "Client";
			udp.Connect("127.0.0.1", 888);

			IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

			while (true)
			{
				try
				{
					// Отправка сообщения
					string message = "Привет, сервер!";
					byte[] sendByte = Encoding.UTF8.GetBytes(message);
					udp.Send(sendByte, sendByte.Length);
					Console.WriteLine($"[Клиент] Отправлено: {message}");

					// Ожидание ответа от сервера
					byte[] responseBytes = udp.Receive(ref serverEndPoint);
					string response = Encoding.UTF8.GetString(responseBytes);
					Console.WriteLine($"[Клиент] Получен ответ от сервера: {response}");

					Thread.Sleep(2000);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Клиент] Ошибка: {ex.Message}");
				}
			}
		}
	}
}
