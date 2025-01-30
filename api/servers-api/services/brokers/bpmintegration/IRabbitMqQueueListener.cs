using servers_api.models.responce;

namespace servers_api.services.brokers.bpmintegration
{
	/// <summary>
	/// Интерфейс listener из bpm.
	/// </summary>
	public interface IRabbitMqQueueListener
	{
		Task<ResponceIntegration> StartListeningAsync(string queueName, CancellationToken stoppingToken);
		void StopListening();
	}
}