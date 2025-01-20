using servers_api.Models;

namespace servers_api.Services.Brokers;

    public interface IRabbitMqQueueManager
    {
        Task<ResponceIntegration> CreateQueues(string inQueue, string outQueue);
    }
