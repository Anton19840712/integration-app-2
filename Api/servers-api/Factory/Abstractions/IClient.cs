namespace servers_api.Factory.Abstractions;

    public interface IClient
    {
        Task ConnectToServerAsync(string host, int port);
    }
