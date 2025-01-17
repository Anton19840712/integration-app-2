namespace servers_api.Models
{
	public class CombinedModel
    {
        public string InQueueName { get; set; }
        public string OutQueueName { get; set; }
        public string Protocol { get; set; }
        public string InternalModel { get; set; }
        public DataOptions DataOptions { get; set; }
    }
}
