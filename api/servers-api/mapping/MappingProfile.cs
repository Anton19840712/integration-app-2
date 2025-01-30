using AutoMapper;
using servers_api.models.internallayerusage.common;
using servers_api.models.internallayerusage.instance;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<CombinedModel, ClientInstanceModel>()
			.ForMember(dest => dest.Protocol, opt => opt.MapFrom(src => src.Protocol))
			.ForMember(dest => dest.DataFormat, opt => opt.MapFrom(src => src.DataFormat))
			.ForMember(dest => dest.InQueueName, opt => opt.MapFrom(src => src.InQueueName))
			.ForMember(dest => dest.OutQueueName, opt => opt.MapFrom(src => src.OutQueueName))
			.ForMember(dest => dest.Host, opt => opt.MapFrom(src => src.DataOptions.ServerDetails.Host))
			.ForMember(dest => dest.Port, opt => opt.MapFrom(src => src.DataOptions.ServerDetails.Port.GetValueOrDefault()))
			.ForMember(dest => dest.ClientConnectionSettings, opt => opt.MapFrom(src => src.ConnectionSettings.ClientConnectionSettings));

		CreateMap<CombinedModel, ServerInstanceModel>()
			.ForMember(dest => dest.Protocol, opt => opt.MapFrom(src => src.Protocol))
			.ForMember(dest => dest.DataFormat, opt => opt.MapFrom(src => src.DataFormat))
			.ForMember(dest => dest.InQueueName, opt => opt.MapFrom(src => src.InQueueName))
			.ForMember(dest => dest.OutQueueName, opt => opt.MapFrom(src => src.OutQueueName))
			.ForMember(dest => dest.Host, opt => opt.MapFrom(src => src.DataOptions.ServerDetails.Host))
			.ForMember(dest => dest.Port, opt => opt.MapFrom(src => src.DataOptions.ServerDetails.Port.GetValueOrDefault()))
			.ForMember(dest => dest.ServerConnectionSettings, opt => opt.MapFrom(src => src.ConnectionSettings.ServerConnectionSettings));
	}
}
