using servers_api.models.response;

namespace servers_api.main.facades
{
	public interface IMessageFacade
	{
		Task<ResponseIntegration> GetLastMessageAsync(CancellationToken stoppingToken);
	}
}
