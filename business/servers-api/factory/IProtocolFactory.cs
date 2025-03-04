namespace servers_api.factory
{
	public interface IProtocolFactory
	{
		UpInstanceByProtocolFactory GetFactory(string protocol);
	}
}
