namespace servers_api.Factory.Abstractions
{
    /// <summary>
    /// Если мы собираемся настраивать динамический шлюз в качестве сервера, то получается, что клиент внешнего контура должен будет знать, на каком адресе был в этот
    /// раз поднят сервер для подготовки ответов клиенту во внешний контур.
    /// </summary>
    public interface IServer
    {
		void UpServer (string host, int? port);
		Task SendServerAddress(string host, int? port);
    }
}
