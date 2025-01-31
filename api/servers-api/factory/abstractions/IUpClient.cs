using servers_api.models.internallayerusage.instance;
using servers_api.models.responce;

namespace servers_api.factory.abstractions
{
	/// <summary>
	/// Интерфейс создания instance as a client node.
	/// </summary>
	public interface IUpClient
	{
		Task<ResponceIntegration> ConnectToServerAsync(
			ClientInstanceModel instanceModel,
			CancellationToken cancellationToken);
	}
}
