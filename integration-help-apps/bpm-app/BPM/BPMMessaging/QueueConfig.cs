using MongoDB.Bson;

namespace BPMMessaging
{
	public class QueueConfig
	{
		public ObjectId Id { get; set; }
		public string IncomingQueue { get; set; }
		public string OutgoingQueue { get; set; }
		public string ExpectedSchema { get; set; } // JSON-схема ожидаемых данных
		public bool IsActive { get; set; } = true; // Флаг активности, активна ли данная схема
	}
}
