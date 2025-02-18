namespace BPMMessaging
{
	public class QueueMonitorService
	{
		private readonly RabbitMqListenerManager _listenerManager;
		private readonly Timer _timer;

		public QueueMonitorService(RabbitMqListenerManager listenerManager)
		{
			_listenerManager = listenerManager;
			_timer = new Timer(async _ => await CheckForNewQueues(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
		}

		private async Task CheckForNewQueues()
		{
			await _listenerManager.StartListenersAsync();
		}
	}
}
