using AutoMapper;
using BPMMessaging.models.dtos;
using BPMMessaging.models.entities;

namespace BPMMessaging.mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// Маппинг для IncidentEntity
			CreateMap<ParsedModel, IncidentEntity>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
				.ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(_ => DateTime.UtcNow));

			// Маппинг для TeachingEntity
			CreateMap<ParsedModel, TeachingEntity>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
				.ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(_ => DateTime.UtcNow));
		}
	}
}
