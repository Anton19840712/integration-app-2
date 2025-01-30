using servers_api.models.internallayerusage.common;
using servers_api.models.responce;

namespace servers_api.Services.InternalSystems
{
	/// <summary>
	/// Сервис, который является 4 ым по списку в процессе обеспечения интеграции.
	/// Он ответственен за обучение bpm: отсылает в нее модель, которая сохраняется в ее базу 
	/// </summary>
	public interface ITeachService
	{
		Task<ResponceIntegration> TeachBPMNAsync(CombinedModel model, CancellationToken token);
	}
}