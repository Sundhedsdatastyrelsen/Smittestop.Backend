using AutoMapper;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using System;

namespace DIGNDB.App.SmitteStop.API.Mappers
{
    public class CovidStatisticsMapper : Profile
    {
        public CovidStatisticsMapper()
        {
            CreateMap<Tuple<SSIStatistics, SSIStatisticsVaccination, ApplicationStatistics>, CovidStatisticsDto>()
                .ForMember(x => x.SSIStatistics, cfg => cfg.MapFrom(x => x.Item1))
                .ForMember(x => x.SsiStatisticsVaccination, cfg => cfg.MapFrom(x => x.Item2))
                .ForMember(x => x.AppStatistics, cfg => cfg.MapFrom(x => x.Item3));
        }
    }
}