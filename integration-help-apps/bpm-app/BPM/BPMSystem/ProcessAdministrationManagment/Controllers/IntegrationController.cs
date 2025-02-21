using AutoMapper;
using BPMMessaging.models;
using BPMMessaging.parsing;
using BPMMessaging.repository;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[Route("Integration")]
[ApiController]
public class IntegrationController : ControllerBase
{
	private readonly IMongoRepository<TeachingEntity> _teachingRepository;
	private readonly IJsonParsingService _jsonParsingService;

	public IntegrationController(
		IMongoRepository<TeachingEntity> teachingRepository,
		IJsonParsingService jsonParsingService,
		IMapper mapper)
	{
		_teachingRepository = teachingRepository;
		_jsonParsingService = jsonParsingService;
	}

	[HttpPost("save")]
	public async Task<ActionResult> SaveTeachingModelAsync([FromBody] JsonElement model)
	{
		try
		{
			// Парсим JSON в TeachingEntity
			var parsedModel = _jsonParsingService.ParseJson<TeachingEntity>(model);

			// Сохраняем в MongoDB
			await _teachingRepository.InsertAsync(parsedModel);

			return Ok(new { Message = "TeachingEntity успешно сохранена" });
		}
		catch (Exception ex)
		{
			return BadRequest(new { Error = "Ошибка при обработке модели", Details = ex.Message });
		}
	}
}
