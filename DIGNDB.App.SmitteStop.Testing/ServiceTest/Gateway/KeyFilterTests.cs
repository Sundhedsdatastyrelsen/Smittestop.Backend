using AutoMapper;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Configuration;
using FederationGatewayApi.Contracts;
using FederationGatewayApi.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest.Gateway
{
    public class KeyFilterTests
    {
        private IKeyFilter _keyFilter;
        private ExposureKeyMock ExposureKeyMock { get; set; }
        private IMapper _keyMapper;
        private Mock<IKeyValidator> _keyValidator;
        private Mock<ITemporaryExposureKeyRepository> _repository;
        private Mock<ICountryRepository> _countryRepository;
        private SetupMockedServices _mockServices;
        private Mock<IKeyValidationConfigurationService> _configurationMock;

        private const int DaysOffset = 14;

        [SetUp]
        public void Init()
        {
            ExposureKeyMock = new ExposureKeyMock();
            _mockServices = new SetupMockedServices();
            _keyValidator = new Mock<IKeyValidator>(MockBehavior.Strict);
            _repository = new Mock<ITemporaryExposureKeyRepository>(MockBehavior.Strict);
            _countryRepository = new Mock<ICountryRepository>(MockBehavior.Strict);
            _mockServices.SetupMapperAndCountryRepositoryMock(_countryRepository);
            _keyMapper = _mockServices.CreateAutoMapperWithDependencies(_countryRepository.Object);
            _mockServices.SetupKeyValidatorMock(_keyValidator);
            _mockServices.SetupTemopraryExposureKeyRepositoryMock(_repository);

            _configurationMock = new Mock<IKeyValidationConfigurationService>();
            _configurationMock.Setup(m => m.GetConfiguration())
                .Returns(new KeyValidationConfiguration() { OutdatedKeysDayOffset = DaysOffset });
            
            _keyFilter = new KeyFilter(_keyMapper, _keyValidator.Object);
        }

        [Test]
        public void KeysAreValidatedProperly()
        {
            var keyList = ExposureKeyMock.MockListOfTemporaryExposureKeys();

            const int numberOfInvalidKeys = 4;

            for (var i = 0; i < numberOfInvalidKeys; i++)
            {
                keyList.Add(ExposureKeyMock.MockInvalidKey());
            }

            var filteredList = _keyFilter.ValidateKeys(keyList, out _);
            Assert.That(filteredList.Count == keyList.Count - numberOfInvalidKeys);
        }

        [Test]
        public void KeysAreMappedProperly()
        {
            var keyList = ExposureKeyMock.MockListOfTemporaryExposureKeyDto();
            var keyMappedList = _keyFilter.MapKeys(keyList);

            for (int i = 0; i < ExposureKeyMock.MockListLength; i++)
            {
                Assert.That(keyList[i].KeyData.SequenceEqual(keyMappedList[i].KeyData));
                Assert.That(keyList[i].RollingPeriod == Convert.ToUInt32(keyMappedList[i].RollingPeriod));
            }
        }
    }
}
