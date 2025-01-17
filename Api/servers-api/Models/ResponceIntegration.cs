namespace servers_api.Models
{
    /// <summary>
    /// Финальный ответ обобщенной модели, говорящий, что все процессы по созданию условий для 
    /// осуществления интеграций прошли успешно.
    /// </summary>
    public class ResponceIntegration
    {
        public string Message { get; set; }
        public bool Result { get; set; }
    }
}
