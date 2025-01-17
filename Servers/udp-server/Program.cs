using System.Net.Sockets;
using System.Text;

class UdpServer
{
    private const int Port = 9876; // Порт для UDP сервера

    static async Task Main(string[] args)
    {
        using (var udpClient = new UdpClient(Port))
        {
            Console.WriteLine($"UDP сервер запущен на порту {Port}.");

            while (true)
            {
                var result = await udpClient.ReceiveAsync(); // Ожидание получения сообщения
                var receivedMessage = Encoding.UTF8.GetString(result.Buffer); // Декодирование сообщения
                Console.WriteLine($"Получено сообщение от {result.RemoteEndPoint}: {receivedMessage}");

                if (receivedMessage.Equals("ping", StringComparison.OrdinalIgnoreCase))
                {
                    var responseMessage = "pong"; // Формирование ответа
                    var responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                    await udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint); // Отправка ответа
                    Console.WriteLine($"Отправлено сообщение: {responseMessage}");
                }
            }
        }
    }
}

