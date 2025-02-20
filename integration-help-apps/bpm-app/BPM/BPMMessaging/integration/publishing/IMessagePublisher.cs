namespace BPMMessaging.integration.Publishing
{
	public interface IMessagePublisher
	{
		Task PublishAsync(string eventType, IntegrationEntity payload);
	}
}