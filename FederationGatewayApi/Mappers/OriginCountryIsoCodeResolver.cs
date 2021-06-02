using AutoMapper;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using FederationGatewayApi.Models;
using Microsoft.Extensions.Logging;

namespace FederationGatewayApi.Mappers
{
    public class OriginCountryIsoCodeResolver : IValueResolver<TemporaryExposureKeyGatewayDto, TemporaryExposureKey, Country>
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<OriginCountryIsoCodeResolver> _logger;

        public OriginCountryIsoCodeResolver(ICountryRepository countryRepository, ILogger<OriginCountryIsoCodeResolver> logger)
        {
            _countryRepository = countryRepository;
            _logger = logger;
        }

        public Country Resolve(TemporaryExposureKeyGatewayDto source, TemporaryExposureKey destination, Country destMember, ResolutionContext context)
        {
            var isoCode = source.Origin;
            var country = _countryRepository.FindByIsoCode(source.Origin);
            if (country == null)
            {
                _logger.LogWarning($"Country code not found in the database: {source.Origin}");
            }
            return country;
        }
    }
}
