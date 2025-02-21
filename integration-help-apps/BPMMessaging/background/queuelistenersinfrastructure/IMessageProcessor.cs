namespace BPMMessaging.background.queuelistenersinfrastructure
{
	public interface IMessageProcessor
	{
		Task ProcessMessageAsync(string queueName, string message);
	}
}
