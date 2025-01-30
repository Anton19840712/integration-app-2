namespace servers_api.models.configurationsettings
{
	public class BaseConnectionSettings
	{
		public int AttemptsToFindBus { get; set; }
		public int BusResponseWaitTimeMs { get; set; }
		public int BusProcessingTimeMs { get; set; }
		public int BusReconnectDelayMs { get; set; }
		public int BusIdleTimeoutMs { get; set; }
	}
}
