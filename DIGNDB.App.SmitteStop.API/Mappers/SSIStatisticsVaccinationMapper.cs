using AutoMapper;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;

namespace FederationGatewayApi.Mappers
{
    public class SSIStatisticsVaccinationMapper : Profile
    {
        public SSIStatisticsVaccinationMapper()
        {
            CreateMap<SSIStatisticsVaccination, SSIStatisticsVaccinationDto>()
                .ForMember(x => x.EntryDate,
                    opts => opts.MapFrom(x => x.Date));
        }
    }
}