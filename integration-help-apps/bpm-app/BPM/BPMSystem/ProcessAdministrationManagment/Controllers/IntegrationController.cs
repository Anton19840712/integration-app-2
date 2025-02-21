using AutoMapper;
using BPMMessaging.models.dtos;
using BPMMessaging.models.entities;
using BPMMessaging.parsing;
using BPMMessaging.repository;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[Route("Integration")]
[ApiController]
public class IntegrationController : ControllerBase
{
	private readonly IMongoRepository<TeachingEntity> _teachingRepository;
	private readonly IMongoRepository<OutboxMessage> _outboxRepository;
	private readonly IJsonParsingService _jsonParsingService;

	public IntegrationController(
		IMongoRepository<TeachingEntity> teachingRepository,
		IMongoRepository<OutboxMessage> outboxRepository,
		IJsonParsingService jsonParsingService,
		IMapper mapper)
	{
		_teachingRepository = teachingRepository;
		_outboxRepository = outboxRepository;
		_jsonParsingService = jsonParsingService;
	}

	[HttpPost("save")]
	public async Task<ActionResult> SaveTeachingModelAsync([FromBody] JsonElement model)
	{
		try
		{
			var parsedModel = _jsonParsingService.ParseJson<TeachingEntity>(model);

			await _teachingRepository.InsertAsync(parsedModel);

			var outboxMessage = new OutboxMessage
			{
				OutQueue = parsedModel.OutQueueName,
				InQueue = parsedModel.InQueueName,
				Payload = parsedModel.IncomingModel,
				IsProcessed = false
			};

			await _outboxRepository.InsertAsync(outboxMessage);
			return Ok(new { Message = "TeachingEntity успешно сохранена" });
		}
		catch (Exception ex)
		{
			return BadRequest(new { Error = "Ошибка при обработке модели", Details = ex.Message });
		}
	}
}
