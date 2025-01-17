namespace servers_api.start
{
	/// <summary>
	/// Запускает dll, которая находится по определенному адресу на диске (сервере)
	/// </summary>
	public interface ITCPServerRunner
	{
		void RunTcpServer(string host, int? port);
	}
}
