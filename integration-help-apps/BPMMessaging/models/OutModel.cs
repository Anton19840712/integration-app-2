namespace BPMMessaging.models
{
	public class OutModel
	{
		public Guid Id { get; set; }
		public string InQueueName { get; set; }
		public string OutQueueName { get; set; }
		public string IncomingModel { get; set; }
	}
}
