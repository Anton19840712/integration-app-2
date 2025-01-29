using servers_api.models;

namespace servers_api.factory.abstractions
{
	/// <summary>
	/// Если мы собираемся настраивать динамический шлюз в качестве сервера, то получается, что клиент внешнего контура должен будет знать, на каком адресе был в этот
	/// раз поднят сервер для подготовки ответов клиенту во внешний контур.
	/// </summary>
	public interface IUpServer
	{
	    Task<ResponceIntegration> UpServerAsync(string host, int? port, CancellationToken cancellationToken);
	}
}
