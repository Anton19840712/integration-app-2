
using servers_api.models;
using servers_api.Models;

namespace servers_api.Services.Connectors
{
	public interface ISenderService
	    {
	        Task<ResponceIntegration> UpAsync(
	            CombinedModel parsedModel,
	            CancellationToken stoppingToken);
	    }
}