using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace UdpServerApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var config = BuildConfig();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

			string messageToSend = config["UdpSettings:Message"];
			if (string.IsNullOrWhiteSpace(messageToSend))
			{
				Log.Error("❌ Сообщение из конфигурации не загружено!");
				return;
			}

			int port = 5018;
			var udpServer = new UdpServer(port, messageToSend);

			Log.Information("Запуск UDP-сервера на порту {Port}...", port);
			var cts = new CancellationTokenSource();

			var serverTask = udpServer.StartAsync(cts.Token);

			Console.WriteLine("Нажмите любую клавишу для остановки сервера.");
			Console.ReadKey();

			cts.Cancel();
			await serverTask;

			Log.Information("UDP-сервер остановлен.");
		}

		static IConfiguration BuildConfig()
		{
			return new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
		}
	}

	public class UdpServer
	{
		private readonly int _port;
		private readonly string _messageToSend;
		private UdpClient _udpServer;
		private CancellationTokenSource _cts;
		private readonly HashSet<IPEndPoint> _clients = new();

		public UdpServer(int port, string messageToSend)
		{
			_port = port;
			_messageToSend = messageToSend;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_udpServer = new UdpClient(_port);

			Log.Information("UDP-сервер запущен на порту {Port}", _port);

			try
			{
				while (!_cts.Token.IsCancellationRequested)
				{
					var result = await _udpServer.ReceiveAsync();

					string received = Encoding.UTF8.GetString(result.Buffer);
					var clientEndPoint = result.RemoteEndPoint;

					Log.Information("Получено сообщение от клиента {Client}: {Message}", clientEndPoint, received);

					if (_clients.Add(clientEndPoint))
					{
						Log.Information("Добавлен новый клиент: {Client}", clientEndPoint);
					}

					foreach (var client in _clients)
					{
						byte[] data = Encoding.UTF8.GetBytes(_messageToSend);
						await _udpServer.SendAsync(data, data.Length, client);
						Log.Information("Отправлено сообщение клиенту {Client}: {Message}", client, _messageToSend);
					}

					await Task.Delay(3000, _cts.Token);
				}
			}
			catch (OperationCanceledException)
			{
				Log.Information("UDP-сервер остановлен по запросу.");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Ошибка в UDP-сервере");
			}
			finally
			{
				_udpServer?.Close();
				Log.Information("Сервер завершил работу.");
			}
		}
	}
}
