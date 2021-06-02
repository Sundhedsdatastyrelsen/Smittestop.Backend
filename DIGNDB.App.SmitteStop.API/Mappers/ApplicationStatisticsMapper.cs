using AutoMapper;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;

namespace DIGNDB.App.SmitteStop.API.Mappers
{
    public class ApplicationStatisticsMapper : Profile
    {
        public ApplicationStatisticsMapper()
        {
            CreateMap<ApplicationStatistics, AppStatisticsDto>()
                .ForMember(x => x.NumberOfPositiveTestsResultsLast7Days,
                    opts => opts.MapFrom(x => x.PositiveResultsLast7Days))
                .ForMember(x => x.NumberOfPositiveTestsResultsTotal,
                    opts => opts.MapFrom(x => x.PositiveTestsResultsTotal))
                .ForMember(x => x.SmittestopDownloadsTotal,
                    opts => opts.MapFrom(x => x.TotalSmittestopDownloads));
        }
    }
}