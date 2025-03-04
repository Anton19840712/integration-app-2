namespace servers_api.factory
{
	public class ProtocolFactory : IProtocolFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public ProtocolFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public UpInstanceByProtocolFactory GetFactory(string protocol)
		{
			return protocol.ToLower() switch
			{
				"tcp" => _serviceProvider.GetRequiredService<TcpFactory>(),
				"udp" => _serviceProvider.GetRequiredService<UdpFactory>(),
				_ => throw new NotSupportedException($"Протокол {protocol} не поддерживается")
			};
		}
	}
}
