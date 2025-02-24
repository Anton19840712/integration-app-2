namespace servers_api.main.facades
{
	public interface IQueueFacade
	{
		Task StartListeningAsync(string outQueue, CancellationToken stoppingToken);
	}
}
