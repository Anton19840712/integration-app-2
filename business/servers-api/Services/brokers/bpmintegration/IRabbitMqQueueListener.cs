namespace servers_api.services.brokers.bpmintegration
{
	public interface IRabbitMqQueueListener
	{
		Task StartListeningAsync(
			string queueOutName,
			CancellationToken stoppingToken);
		void StopListening();
	}
}
