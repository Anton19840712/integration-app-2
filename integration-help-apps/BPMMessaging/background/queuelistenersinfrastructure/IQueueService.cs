namespace BPMMessaging.background.queuelistenersinfrastructure
{
	public interface IQueueService
	{
		void StartListener(string queueName);
	}
}