namespace servers_api.factory.abstractions
{
	/// <summary>
	/// Создаем или клиент или сервер определенного протокола.
	/// </summary>
	public abstract class ProtocolFactory
	{
	    public abstract IUpServer CreateServer();
	    public abstract IUpClient CreateClient();
	}
}
