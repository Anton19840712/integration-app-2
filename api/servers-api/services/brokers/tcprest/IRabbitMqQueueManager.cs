using servers_api.models.responce;

namespace servers_api.services.brokers.tcprest
{
	/// <summary>
	/// Создатель очередей.
	/// </summary>
	public interface IRabbitMqQueueManager
	{
		Task<ResponceIntegration> CreateQueuesAsync(string inQueue, string outQueue);
	}
}
