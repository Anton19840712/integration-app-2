using servers_api.models.internallayerusage.instance;
using servers_api.models.responce;

namespace servers_api.factory.abstractions
{
	/// <summary>
	/// Класс, который поднимает в динамическом шлюзе
	/// согласно входящей информации либо клиент, либо сервер определенного соединения.
	/// </summary>
	public class ProtocolManager : IProtocolManager
	{
		public async Task<ResponceIntegration> ConfigureAsync(InstanceModel instanceModel)
		{
			if (instanceModel is ClientInstanceModel clientModel)
			{
				// Логика для клиента
				return await ConfigureClientAsync(clientModel);
			}
			else if (instanceModel is ServerInstanceModel serverModel)
			{
				// Логика для сервера
				return await ConfigureServerAsync(serverModel);
			}

			return new ResponceIntegration
			{
				Message = "Неизвестный тип инстанса",
				Result = false
			};
		}

		private Task<ResponceIntegration> ConfigureClientAsync(ClientInstanceModel clientModel)
		{
			// Логика для настройки клиента
			// Здесь используется clientModel.Host, clientModel.Port и другие параметры
			return Task.FromResult(new ResponceIntegration { Result = true });
		}

		private Task<ResponceIntegration> ConfigureServerAsync(ServerInstanceModel serverModel)
		{
			// Логика для настройки сервера
			// Здесь используется serverModel.Host, serverModel.Port и другие параметры
			return Task.FromResult(new ResponceIntegration { Result = true });
		}
	}
}


