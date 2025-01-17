namespace servers_api.ping
{
	public interface ITcpPingClientService
	{
		Task<string> PingServerAsync(string host, int? port);
	}
}
