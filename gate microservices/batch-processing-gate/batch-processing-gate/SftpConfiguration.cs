namespace sftp_dynamic_gate_app
{
	static class SftpConfiguration
	{
		/// <summary>
		/// Регистрация сервисов общего назначения.
		/// </summary>
		public static IServiceCollection AddSftpServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<SftpSettings>(configuration.GetSection("SftpSettings"));
			services.AddTransient<FileHashService>();

			return services;
		}
	}
}
