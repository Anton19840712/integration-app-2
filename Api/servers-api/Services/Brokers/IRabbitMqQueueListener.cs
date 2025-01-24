using servers_api.Models;

namespace servers_api.Services.Brokers
{
	    public interface IRabbitMqQueueListener
	    {
	        Task<ResponceIntegration> StartListeningAsync(string queueName, CancellationToken stoppingToken);
	        void StopListening();
	    }
}