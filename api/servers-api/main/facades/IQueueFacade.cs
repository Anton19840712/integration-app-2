using servers_api.models.response;

namespace servers_api.main.facades
{
	public interface IQueueFacade
	{
		Task<ResponseIntegration> CreateQueuesAsync(string inQueue, string outQueue, CancellationToken stoppingToken);
		Task StartListeningAsync(string outQueue, CancellationToken stoppingToken);
	}
}
