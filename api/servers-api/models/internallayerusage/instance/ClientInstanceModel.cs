using servers_api.models.configurationsettings;

namespace servers_api.models.internallayerusage.instance
{
	/// <summary>
	/// Модель для клиента
	/// </summary>
	public class ClientInstanceModel : InstanceModel
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public ClientSettings ClientConnectionSettings { get; set; }
	}
}
