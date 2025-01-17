namespace servers_api.Factory.Abstractions
{
    /// <summary>
    /// Создаем или клиент или сервер определенного протокола.
    /// </summary>
    public abstract class ProtocolFactory
    {
        public abstract IServer CreateServer();
        public abstract IClient CreateClient();
    }
}
