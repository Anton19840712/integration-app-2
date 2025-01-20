namespace servers_api.Factory.Abstractions;

    public interface IClient
    {
        void ConnectToServer(string host, int port);
    }
