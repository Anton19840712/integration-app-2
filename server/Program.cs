﻿using System.Net;
using System.Net.Sockets;
using System.Text;

Console.Title = "TCP Server";
Console.WriteLine("Сервер запускается...");

int port = 5000; // Порт для подключения
var listener = new TcpListener(IPAddress.Any, port);

listener.Start();
Console.WriteLine($"Сервер запущен и слушает порт {port}");

while (true)
{
	try
	{
		// Принимаем клиента
		var client = await listener.AcceptTcpClientAsync();
		Console.WriteLine($"Клиент подключен: {client.Client.RemoteEndPoint}");

		using var stream = client.GetStream();
		var buffer = new byte[256];

		// Читаем сообщение от клиента
		int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
		var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
		Console.WriteLine($"Получено сообщение: {message}");

		// Если сообщение "ping", отправляем "pong"
		if (message.Equals("ping", StringComparison.OrdinalIgnoreCase))
		{
			var responseMessage = Encoding.UTF8.GetBytes("pong");
			await stream.WriteAsync(responseMessage, 0, responseMessage.Length);
			Console.WriteLine("Ответ отправлен: pong");
		}
		else
		{
			Console.WriteLine("Неизвестное сообщение.");
		}

		client.Close();
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Ошибка: {ex.Message}");
	}
}

