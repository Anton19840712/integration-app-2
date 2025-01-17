namespace servers_api.Models
{
    /// <summary>
    /// Очереди, которые будут созданы для пробрасывания в них in и out сообщений.
    /// </summary>
    public class QueuesNames
    {
        public string InQueueName { get; set; }
        public string OutQueueName { get; set; }
    }
}
