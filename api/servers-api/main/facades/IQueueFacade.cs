namespace servers_api.main.facades
{
	public interface IQueueFacade
	{
		Task CreateQueuesAsync(string inQueue, string outQueue, CancellationToken stoppingToken);
		Task StartListeningAsync(string outQueue, CancellationToken stoppingToken);
	}
}
