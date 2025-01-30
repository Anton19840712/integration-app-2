namespace servers_api.models.configurationsettings
{
	public class ClientSettings : BaseConnectionSettings
	{
		public int AttemptsToFindExternalServer { get; set; }
		public int ConnectionTimeoutMs { get; set; }
	}
}
