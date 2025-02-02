using servers_api.models.configurationsettings;
using servers_api.models.internallayer.common;

namespace servers_api.models.internallayer.instance
{
	/// <summary>
	/// Модель для клиента
	/// </summary>
	public class ClientInstanceModel : InstanceModel
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public ClientSettings ClientConnectionSettings { get; set; }
		public ConnectionEndpoint ServerHostPort { get; set; }
	}
}
