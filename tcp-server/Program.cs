using System.Net.Sockets;
using System.Net;
using System.Text;

Console.Title = "TCP Server";
Console.WriteLine("������ �����������...");

int port = 5018; // ���� ��� �����������
var item = "127.0.0.1"; // IP-�����
var listener = new TcpListener(IPAddress.Any, port); // �������������� ������ � IPAddress


listener.Start();
Console.WriteLine($"������ ������� � ������� ���� {item} {port} ");

while (true)
{
	try
	{
		// ��������� �������
		var client = await listener.AcceptTcpClientAsync();
		Console.WriteLine($"������ ���������: {client.Client.RemoteEndPoint}");

		using var stream = client.GetStream();
		var buffer = new byte[256];

		// ������ ��������� �� �������
		int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
		var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
		Console.WriteLine($"�������� ���������: {message}");

		// ���� ��������� "ping", ���������� "pong"
		if (message.Equals("ping", StringComparison.OrdinalIgnoreCase))
		{
			var responseMessage = Encoding.UTF8.GetBytes("pong");
			await stream.WriteAsync(responseMessage, 0, responseMessage.Length);
			Console.WriteLine("����� ���������: pong");
		}
		else
		{
			Console.WriteLine("����������� ���������.");
		}

		client.Close();
	}
	catch (Exception ex)
	{
		Console.WriteLine($"������: {ex.Message}");
	}
}