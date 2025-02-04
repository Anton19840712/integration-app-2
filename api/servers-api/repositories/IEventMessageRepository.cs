using servers_api.events;

namespace servers_api.repositories
{
	public interface IEventMessageRepository
	{
		Task SaveEventMessageAsync(IncidentCreated eventMessage);
	}
}
