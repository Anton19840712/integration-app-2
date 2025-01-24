namespace servers_api.Factory.Abstractions
{
	
	    /// <summary>
	    /// Если мы собираемся настраивать динамический шлюз в качестве сервера, то получается, что клиент внешнего контура должен будет знать, на каком адресе был в этот
	    /// раз поднят сервер для подготовки ответов клиенту во внешний контур.
	    /// </summary>
	    public interface IServer
	    {
	        Task UpServerAsync(string host, int? port, CancellationToken cancellationToken);
		}
}
