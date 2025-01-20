using servers_api.Factory.Abstractions;

namespace servers_api.Factory.UDP;

    public class UdpServer : IServer
    {
	public void UpServer(string host, int? port)
	{
		throw new NotImplementedException();
	}

	public async Task SendServerAddress(string host, int? port)
	{
		// Возвращаем уже завершённую задачу
		await Task.CompletedTask;
	}
}
