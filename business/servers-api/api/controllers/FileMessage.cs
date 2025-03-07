namespace servers_api.api.controllers
{
	public class FileMessage
	{
		public byte[] FileContent { get; set; } // Бинарное содержимое файла
		public string FileExtension { get; set; } // Расширение файла
	}
}
