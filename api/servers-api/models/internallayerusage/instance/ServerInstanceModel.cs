using servers_api.models.configurationsettings;

namespace servers_api.models.internallayerusage.instance
{
	/// <summary>
	/// Модель для сервера
	/// </summary>
	public class ServerInstanceModel : InstanceModel
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public ServerSettings ServerConnectionSettings { get; set; }
	}
}
