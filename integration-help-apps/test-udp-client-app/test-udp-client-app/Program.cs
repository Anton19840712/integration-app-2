using System.Net;
using System.Net.Sockets;
using System.Text;

class UdpClientApp
{
	private const int Port = 5001;
	private const string ServerIp = "127.0.0.1"; // Адрес сервера

	static void Main()
	{
		var udpClient = new UdpClient();
		var serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), Port);

		try
		{
			// Сообщение, которое будет отправлено серверу
			string message = "Привет, сервер!";
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);

			// Отправка сообщения серверу
			udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
			Console.WriteLine($"Сообщение отправлено на {ServerIp}:{Port}");

			// Получение ответа от сервера
			var receivedData = udpClient.Receive(ref serverEndPoint);
			string serverResponse = Encoding.UTF8.GetString(receivedData);
			Console.WriteLine($"Ответ от сервера: {serverResponse}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка: {ex.Message}");
		}
	}
}
