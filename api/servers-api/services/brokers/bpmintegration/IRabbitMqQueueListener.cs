
public interface IRabbitMqQueueListener
{
	Task StartListeningAsync(
		string queueOutName,
		CancellationToken stoppingToken);
	void StopListening();
}