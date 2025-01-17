using System.Net.Sockets;
using System.Text;

Console.Title = "TCP Client";
Console.WriteLine("Клиент запускается...");

string host = "127.0.0.1"; // IP-адрес сервера
int port = 5000;          // Порт сервера

try
{
	using var client = new TcpClient();
	Console.WriteLine($"Подключение к серверу {host}:{port}...");
	await client.ConnectAsync(host, port);

	using var stream = client.GetStream();
	var message = "ping";
	var buffer = Encoding.UTF8.GetBytes(message);

	// Отправляем "ping"
	await stream.WriteAsync(buffer, 0, buffer.Length);
	Console.WriteLine($"Отправлено сообщение: {message}");

	// Читаем ответ от сервера
	buffer = new byte[256];
	int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
	var response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

	Console.WriteLine($"Получен ответ от сервера: {response}");
}
catch (Exception ex)
{
	Console.WriteLine($"Ошибка: {ex.Message}");
}

