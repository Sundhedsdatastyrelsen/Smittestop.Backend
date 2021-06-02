using AutoMapper;
using DIGNDB.App.SmitteStop.API.Mappers;
using FederationGatewayApi.Mappers;
using NUnit.Framework;

namespace DIGNDB.App.SmitteStop.Testing.MapperTest
{
    [TestFixture]
    public class CovidStatisticsMapperTests
    {
        private MapperConfiguration _config;
        [OneTimeSetUp]
        public void Init()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CovidStatisticsMapper>();
                cfg.AddProfile<ApplicationStatisticsMapper>();
                cfg.AddProfile<SSIStatisticsMapper>();
                cfg.AddProfile<SSIStatisticsVaccinationMapper>();
            });
        }

        [Test]
        public void Mapper_Should_HaveValidConfig()
        {
            //_config.AssertConfigurationIsValid();
        }
    }
}
