using System.Text.Json;
using BPMMessaging.integration.Models;
using BPMMessaging.integration.Services.Save;
using Microsoft.AspNetCore.Mvc;

namespace BPMSystem.ProcessAdministrationManagment.Controllers
{
	[Route($"Integration")]
	[ApiController]
	public class IntegrationController : ControllerBase
	{
		private readonly ISaveService _integrationService;

		public IntegrationController(ISaveService integrationService)
		{
			_integrationService = integrationService;
		}

		[HttpGet($"ping")]
		public async Task<ActionResult<ResponceIntegration>> PingAsync()
		{
			await Task.Delay(1000);

			return new ResponceIntegration
			{
				Message = "Ping successful",
				Result = true
			};
		}

		/// <summary>
		/// Этот метод используется в рамках настройки bpm: 
		/// создается и сохраняется модель, которая содержит модель, с которой будет нужно работать. 
		/// Так же название очередей, из которой будут заслушиваться сообщения и в которую будут отправляться сообщения.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost($"save")]
		public async Task<ActionResult> SaveModelAsync([FromBody] JsonElement model)
		{
			var integration = await _integrationService.SaveIntegrationModelAsync(model);
			return Ok(new { message = nameof(model), id = integration.Id, data = integration });
		}
	}
}
