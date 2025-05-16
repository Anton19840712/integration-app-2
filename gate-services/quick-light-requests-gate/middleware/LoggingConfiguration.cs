using Serilog;

namespace middleware
{
    public static class LoggingConfiguration
    {
		/// <summary>
		/// ��������� Serilog ��� ����������
		/// </summary>
		public static void ConfigureLogging(WebApplicationBuilder builder)
		{
			builder.Host.UseSerilog((ctx, cfg) =>
			{
				cfg.MinimumLevel.Information()
				   .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
				   .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
				   .Enrich.FromLogContext();
			});
		}
	}
}
