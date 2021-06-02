using AutoMapper;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using System.Linq;

namespace FederationGatewayApi.Mappers
{
    public class CountryMapper : Profile
    {
        public CountryMapper()
        {
            CreateMap<Country, CountryDto>()
                .ForMember(m => m.NameInEnglish,
                    o => o.MapFrom(
                        e => e.EntityTranslations.FirstOrDefault(
                            t => t.EntityPropertyName == nameof(CountryDto.NameInEnglish)).Value))
                .ForMember(m => m.NameInDanish,
                o => o.MapFrom(
                    e => e.EntityTranslations.FirstOrDefault(
                        t => t.EntityPropertyName == nameof(CountryDto.NameInDanish)).Value));
        }
    }
}