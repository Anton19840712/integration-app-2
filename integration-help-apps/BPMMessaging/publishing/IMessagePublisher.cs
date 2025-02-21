using BPMMessaging.models;

namespace BPMMessaging.publishing
{
	public interface IMessagePublisher
	{
		Task PublishAsync(string eventType, OutModel payload);
	}
}
