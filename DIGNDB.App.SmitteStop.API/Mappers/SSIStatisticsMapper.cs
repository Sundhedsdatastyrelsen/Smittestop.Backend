using AutoMapper;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;

namespace FederationGatewayApi.Mappers
{
    public class SSIStatisticsMapper : Profile
    {
        public SSIStatisticsMapper()
        {
            CreateMap<SSIStatistics, SSIStatisticsDto>()
                .ForMember(dest => dest.EntryDate,
                    opts => opts.MapFrom(x => x.Date));
        }
    }
}