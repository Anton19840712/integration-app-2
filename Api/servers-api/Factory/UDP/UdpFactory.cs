using servers_api.Factory.Abstractions;

namespace servers_api.Factory.UDP
{
	    public class UdpFactory : ProtocolFactory
	    {
	        public override IServer CreateServer()
	        {
	            // по идее мы поднимаем наш сервер или его пингуем, берем его хост и порт и отправляем обратно в ответе нашему интегратору конфигурации в ответе. 
	            // то есть после этого считается, что зона ответственности настройки взаимодействия между клиентом и
	            // сервером завершена. Хендшейк уже они уже будут делать сами прямо ли сейчас, через месяц 

	            return new UdpServer();
	        }

	        public override IClient CreateClient()
	        {
	            return new UdpClient();
	        }
	    }
}
