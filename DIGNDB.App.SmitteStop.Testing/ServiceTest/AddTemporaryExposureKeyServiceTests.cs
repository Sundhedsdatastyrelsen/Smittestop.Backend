﻿using DIGNDB.App.SmitteStop.Core;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Core.Services;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using DIGNDB.App.SmitteStop.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest
{
    [TestFixture]
    public class AddTemporaryExposureKeyServiceTests
    {
        private Mock<ILogger<AddTemporaryExposureKeyService>> _logger;
        private readonly Mock<ITemporaryExposureKeyRepository> _temporaryExposureKeyRepositoryMock =
            new Mock<ITemporaryExposureKeyRepository>();
        private readonly List<TemporaryExposureKey> _exampleKeyList = new List<TemporaryExposureKey>
        {
            new TemporaryExposureKey{ KeyData = new byte[] { 1, 2, 3 }},
            new TemporaryExposureKey{ KeyData = new byte[] { 2, 3, 1 }},
            new TemporaryExposureKey{ KeyData = new byte[] { 3, 2, 1 }},
            new TemporaryExposureKey{ KeyData = new byte[] { 3, 2, 1 }}
        };

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<AddTemporaryExposureKeyService>>();
        }

        [Test]
        public async Task Test_remove_duplicate_key()
        {
            var addTemporaryExposureKeyService = CreateTestObject();
            var parameters = new TemporaryExposureKeyBatchDto
            {
                appPackageName = string.Empty,
                visitedCountries = new List<string>
                {
                    "CR",
                    "PL",
                    "DK"
                },
                regions = new List<string>
                {
                    "dk"
                }
            };

            var keysList = await addTemporaryExposureKeyService.GetFilteredKeysEntitiesFromDTO(parameters);
            Assert.AreEqual(3, keysList.Count);

        }

        [Test]
        public async Task TestCreateKeysInDatabase()
        {
            var addTemporaryExposureKeyService = CreateTestObject();
            var parameters = new TemporaryExposureKeyBatchDto
            {
                appPackageName = string.Empty,
                visitedCountries = new List<string>
                {
                    "CR",
                    "PL",
                    "DK"
                },
                regions = new List<string>
                {
                    "dk"
                }
            };

            await addTemporaryExposureKeyService.CreateKeysInDatabase(parameters);

            _temporaryExposureKeyRepositoryMock.Verify(mock =>
                mock.AddTemporaryExposureKeys(It.Is<IList<TemporaryExposureKey>>(keys =>
                    keys.All(key => key.KeySource == KeySource.SmitteStopApiVersion2))));

            _temporaryExposureKeyRepositoryMock.Verify(mock =>
               mock.AddTemporaryExposureKeys(It.Is<IList<TemporaryExposureKey>>(keys =>
                   keys.All(key => key.VisitedCountries.Any(country => country.Country.Code.ToLower() == "dk") == false))));
        }

        public AddTemporaryExposureKeyService CreateTestObject()
        {
            _temporaryExposureKeyRepositoryMock.Setup(x => x.GetNextBatchOfKeysWithRollingStartNumberThresholdAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<TemporaryExposureKey>());
            _temporaryExposureKeyRepositoryMock.Setup(x => x.GetNextBatchOfKeysWithRollingStartNumberThresholdAsync(It.IsAny<long>(), 0, It.IsAny<int>())).ReturnsAsync(new List<TemporaryExposureKey>() { new TemporaryExposureKey() });
            var countryRepositoryMock = new Mock<ICountryRepository>();
            countryRepositoryMock.Setup(
                    m => m.FindByIsoCode(It.IsAny<string>()))
                .Returns(new Country()
                {
                    Code = "DK"
                });

            var temporaryExposureKeyCountryRepositoryMock = new Mock<IGenericRepository<TemporaryExposureKeyCountry>>();
            var exposureKeyMapperMock = new Mock<IExposureKeyMapper>();
            exposureKeyMapperMock.Setup(x => x.FromDtoToEntity(It.IsAny<TemporaryExposureKeyBatchDto>())).Returns(_exampleKeyList);

            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("false");
            configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);
            var appSettingsMock = new Mock<IAppSettingsConfig>();
            appSettingsMock.Setup(mock => mock.Configuration).Returns(configuration.Object);
            
            var addTemporaryExposureKeyService = new AddTemporaryExposureKeyService(
                countryRepositoryMock.Object,
                temporaryExposureKeyCountryRepositoryMock.Object,
                exposureKeyMapperMock.Object,
                _temporaryExposureKeyRepositoryMock.Object,
                _logger.Object);

            return addTemporaryExposureKeyService;
        }
    }
}