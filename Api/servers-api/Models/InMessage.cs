namespace servers_api.Models
{
	/// <summary>
	/// Сообщение, которое планируется передаваться через сетевую шину из данного нода.
	/// </summary>
	public class InMessage
	    {
	        public string InternalModel { get; set; }
	        public QueuesNames QueuesNames { get; set; }
	    }
}
