
namespace CommonGateLib.RabbitMQ
{
    public interface IRabbitMqQueueListener
    {
        Task StartListeningAsync(string queueOutName, CancellationToken stoppingToken, string pathForSave = null, Func<string, Task> onMessageReceived = null);
        void StopListening();
    }
}
