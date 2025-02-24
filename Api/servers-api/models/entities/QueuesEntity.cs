namespace servers_api.models.entities
{
	public class QueuesEntity : AuditableEntity
	{
		public string InQueueName { get; set; }
		public string OutQueueName { get; set; }
	}
}
