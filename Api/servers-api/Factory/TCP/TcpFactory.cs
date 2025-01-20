using servers_api.Factory.Abstractions;

namespace servers_api.Factory.TCP;

public class TcpFactory : ProtocolFactory
    {

        public override IServer CreateServer()
        {
            return new TcpServer();
        }

        // если на UI/или в конфигурационном файлы мы выбрали сервер, то я предполагаю, что сервер должен создаться в нашем контуре и отправить
        // свой адрес, где он был запущен на клиент для начала
        // если же мы захотели поднять клиент, который будет обращаться к их внешнему серверу, то в конфигурационной информации мы должны будем получить 
        // адрес сервера, куда мы будем соединяться, обращаться автоматически.
        public override IClient CreateClient()
        {
            return new TcpClient();
        }
    }
