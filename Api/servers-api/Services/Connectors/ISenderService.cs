
using servers_api.Models;

namespace servers_api.Services.Connectors
{
    public interface ISenderService
    {
        Task<ResponceIntegration> RunServerByProtocolTypeAsync(
            CombinedModel parsedModel,
            CancellationToken stoppingToken);
    }
}