using System.Net.Sockets;
using System.Text;

class Program
{
    private const int Port = 9876; // Порт для UDP сервера

    static async Task Main(string[] args)
    {
        using (var udpClient = new UdpClient(Port))
        {
            Console.WriteLine($"UDP сервер запущен на порту {Port}.");

            while (true)
            {
                var result = await udpClient.ReceiveAsync();
                var receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                Console.WriteLine($"Получено сообщение от {result.RemoteEndPoint}: {receivedMessage}");

                if (receivedMessage.Equals("ping", StringComparison.OrdinalIgnoreCase))
                {
                    var responseMessage = "pong";
                    var responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                    await udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                    Console.WriteLine($"Отправлено сообщение: {responseMessage}");
                }
            }
        }
    }
}

