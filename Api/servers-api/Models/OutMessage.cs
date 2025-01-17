using Newtonsoft.Json.Linq;

namespace servers_api.Models
{
	/// <summary>
	/// Сообщение, которое планируется получать из сетевой шины В данный нод.
    /// Это может быть ответ от bpm с той стороны, что модель, которая была туде передана, была принята.
    /// Либо в теории какие-то данные, который должна будет подготовить нам bpm система.
	/// </summary>
	public class OutMessage
    {
        public Guid Id { get; set; }
        public string InQueueName { get; set; }
        public string OutQueueName { get; set; }
        public JObject IncomingModel { get; set; }
    }
}
