using AutoMapper;
using DIGNDB.App.SmitteStop.API;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Testing.ControllersTest.V2
{
    [TestFixture]
    public class CovidStatisticsControllerTests
    {
        private MockRepository _mockRepository;

        private Mock<ILogger<CovidStatisticsController>> _mockLogger;
        private Mock<IApplicationStatisticsRepository> _mockApplicationStatisticsRepository;
        private Mock<ISSIStatisticsRepository> _mockSSIStatisticsRepository;
        private Mock<ISSIStatisticsVaccinationRepository> _mockSSIStatisticsVaccinationRepository;
        private Mock<IMapper> _mockMapper;

        private ApplicationStatistics _appStatisticsEntry;
        private readonly DateTime _ssiPackageDate = new DateTime(2020, 10, 20, 5, 5, 3);
        private readonly DateTime _appPackageDate = new DateTime(2020, 10, 10, 5, 5, 3);
        private SSIStatistics _ssiStatisticsEntry;
        private SSIStatisticsVaccination _ssiStatisticsVaccinationEntry;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _mockLogger = _mockRepository.Create<ILogger<CovidStatisticsController>>(MockBehavior.Loose);
            _mockApplicationStatisticsRepository = _mockRepository.Create<IApplicationStatisticsRepository>();
            _mockSSIStatisticsRepository = _mockRepository.Create<ISSIStatisticsRepository>();
            _mockSSIStatisticsVaccinationRepository = _mockRepository.Create<ISSIStatisticsVaccinationRepository>();
            _mockMapper = _mockRepository.Create<IMapper>(MockBehavior.Loose);
            _appStatisticsEntry = new ApplicationStatistics()
            {
                EntryDate = _appPackageDate,
                Id = 1,
                PositiveResultsLast7Days = 1000,
                PositiveTestsResultsTotal = 2000,
                TotalSmittestopDownloads = 3000
            };
            _ssiStatisticsEntry = new SSIStatistics()
            {
                ConfirmedCasesTotal = 100,
                ConfirmedCasesToday = 200,
                Date = _ssiPackageDate,
                DeathsToday = 300,
                DeathsTotal = 400,
                Id = 1,
                TestsConductedToday = 600,
                TestsConductedTotal = 700
            };
            _ssiStatisticsVaccinationEntry = new SSIStatisticsVaccination
            {
                VaccinationFirst = 6.2,
                VaccinationSecond = 3.1
            };
        }

        private CovidStatisticsController CreateCovidStatisticsController()
        {
            return new CovidStatisticsController(
                _mockLogger.Object,
                _mockApplicationStatisticsRepository.Object,
                _mockSSIStatisticsRepository.Object,
                _mockSSIStatisticsVaccinationRepository.Object,
                _mockMapper.Object);
        }

        [Test]
        public async Task GetCovidStatistics_NoApplicationStatistics_ShouldReturnBadRequest()
        {
            // Arrange
            _mockApplicationStatisticsRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(value: null);
            var covidStatisticsController = CreateCovidStatisticsController();
            string packageDate = null;

            // Act
            var result = await covidStatisticsController.GetCovidStatistics(
                packageDate);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }

        [Test]
        public async Task GetCovidStatistics_FetchingNewestPackage_NoPackagesExist_ShouldReturnNoContent()
        {
            // Arrange
            _mockApplicationStatisticsRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(_appStatisticsEntry);
            _mockSSIStatisticsRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(value: null);
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(value: null);
            var covidStatisticsController = CreateCovidStatisticsController();
            string packageDate = null;

            // Act
            var result = await covidStatisticsController.GetCovidStatistics(packageDate);

            // Assert
            Assert.IsInstanceOf(typeof(NoContentResult), result);
        }

        [Test]
        public async Task GetCovidStatistics_FetchingSpecificPackage_PackagesExist_ShouldReturnPackage()
        {
            // Arrange
            string packageDate = _ssiPackageDate.Date.ToString();
            _mockApplicationStatisticsRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(_appStatisticsEntry);
            _mockSSIStatisticsRepository.Setup(x => x.GetEntryByDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(_ssiStatisticsEntry);
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(_ssiStatisticsVaccinationEntry);
            var covidStatisticsController = CreateCovidStatisticsController();

            // Act
            var result = await covidStatisticsController.GetCovidStatistics(packageDate);

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
        }

        [Test]
        public async Task GetCovidStatistics_FetchingNewestPackage_PackagesExists_ShouldReturnPackage()
        {
            // Arrange
            _mockApplicationStatisticsRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(_appStatisticsEntry);
            _mockSSIStatisticsRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(_ssiStatisticsEntry);
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(_ssiStatisticsVaccinationEntry);
            var covidStatisticsController = CreateCovidStatisticsController();
            string packageDate = null;

            // Act
            var result = await covidStatisticsController.GetCovidStatistics(packageDate);

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
        }

        [Test]
        public async Task GetCovidStatistics_FetchingSpecificPackage_NoPackagesExists_ShouldReturnNoContent()
        {
            // Arrange
            string packageDate = _ssiPackageDate.Date.ToString();
            _mockApplicationStatisticsRepository.Setup(x => x.GetNewestEntryAsync()).ReturnsAsync(_appStatisticsEntry);
            _mockSSIStatisticsRepository.Setup(x => x.GetEntryByDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(value: null);
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(value: null);
            var covidStatisticsController = CreateCovidStatisticsController();

            // Act
            var result = await covidStatisticsController.GetCovidStatistics(
                packageDate);

            // Assert
            Assert.IsInstanceOf(typeof(NoContentResult), result);
        }
    }
}
