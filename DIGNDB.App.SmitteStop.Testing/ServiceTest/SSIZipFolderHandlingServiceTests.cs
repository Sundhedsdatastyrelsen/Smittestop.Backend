using DIGNDB.App.SmitteStop.Core.Adapters;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Jobs.Services;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Exceptions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DIGNDB.App.SmitteStop.Testing.Services
{
    [TestFixture]
    public class SSIZipFolderHandlingServiceTests
    {
        private MockRepository _mockRepository;

        private SSIZipFolderProcessingConfig _mockSSIZipFolderProcessingConfig;
        private Mock<ILoggerAdapter<SSIZipFolderHandlingService>> _mockLogger;
        private Mock<IFileSystem> _mockFileSystem;
        private readonly string _sampleFolderPath = "./aaa";
        private List<string> _sampleListOfFiles;
        private readonly string _sampleZipPackageDatePattern = "[0-9]{4}_[0-9]{2}_[0-9]{2}";
        private readonly string _sampleZipPackageDateParsingFormat = "yyyy_MM_dd";
        private readonly string _sampleInfectionNumberPrefix = "Smittestop_smittetal_";

        private readonly string _sampleZipPackageVaccineDatePattern = "[0-9]{2}[0-9]{2}[0-9]{4}";
        private readonly string _sampleZipPackageVaccineDateParsingFormat = "ddMMyyyy";
        private readonly string _sampleVaccinationNumberPrefix = "covid19-vaccinationsdata-";

        private readonly string _sampleZipStatPackageDatePattern = "[0-9]{4}_[0-9]{2}_[0-9]{2}";
        private readonly string _sampleZipStatPackageDateParsingFormat = "yyyy_MM_dd";
        private readonly string _sampleStatNumberPrefix = "covid19_DB_data_";

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);
            _mockSSIZipFolderProcessingConfig = new SSIZipFolderProcessingConfig();
            _mockLogger = _mockRepository.Create<ILoggerAdapter<SSIZipFolderHandlingService>>(MockBehavior.Loose);
            _mockFileSystem = _mockRepository.Create<IFileSystem>();
            SetupSampleData();
        }

        private void SetupSampleData()
        {
            _sampleListOfFiles = new List<string>
            {
                "Smittestop_smittetal_2020_12_10.zip",
                "Smittestop_smittetal_2020_12_09.zip",
                "Smittestop_smittetal_2020_12_08.zip",
                "Smittestop_smittetal_2020_12_07.zip",
                "Smittestop_smittetal_2020_12_06.zip",
                "Smittestop_smittetal_2020_12_05.zip",        // [5]
                "covid19-vaccinationsdata-10122020-2bak.zip", // [6]
                "covid19-vaccinationsdata-09122020-2bak.zip",
                "covid19-vaccinationsdata-08122020-2bak.zip",
                "covid19_DB_data_2021_03_25.zip"  //[9]
            };
        }

        private void ConfigureMocks()
        {
            _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(x => x.GetFileNamesFromDirectory(_sampleFolderPath)).Returns(_sampleListOfFiles.ToArray);

            _mockSSIZipFolderProcessingConfig.StatisticsZipFolderPath = _sampleFolderPath;

            _mockSSIZipFolderProcessingConfig.ZipPackageDatePattern = _sampleZipPackageDatePattern;
            _mockSSIZipFolderProcessingConfig.ZipPackageDateParsingFormat = _sampleZipPackageDateParsingFormat;
            _mockSSIZipFolderProcessingConfig.InfectionNumbersPrefix = _sampleInfectionNumberPrefix;

            _mockSSIZipFolderProcessingConfig.ZipPackageVaccineDatePattern = _sampleZipPackageVaccineDatePattern;
            _mockSSIZipFolderProcessingConfig.ZipPackageVaccineDateParsingFormat = _sampleZipPackageVaccineDateParsingFormat;
            _mockSSIZipFolderProcessingConfig.VaccinationNumbersPrefix = _sampleVaccinationNumberPrefix;

            _mockSSIZipFolderProcessingConfig.ZipPackageVaccineDatePattern = _sampleZipStatPackageDatePattern;
            _mockSSIZipFolderProcessingConfig.ZipPackageVaccineDateParsingFormat = _sampleZipStatPackageDateParsingFormat;
            _mockSSIZipFolderProcessingConfig.StatisticsPrefix = _sampleStatNumberPrefix;

            _mockLogger.Setup(x => x.LogWarning(It.IsAny<string>())).Verifiable();
        }

        private SSIZipFolderHandlingService CreateService()
        {
            return new SSIZipFolderHandlingService(
                _mockSSIZipFolderProcessingConfig,
                _mockLogger.Object,
                _mockFileSystem.Object);
        }

        [Test]
        public void GetNewestArchivesFromFolder_DirectoryDoesNotExist_ShouldThrowProperException()
        {
            // Arrange
            ConfigureMocks();
            _mockFileSystem.Setup(x => x.DirectoryExists(_sampleFolderPath)).Returns(false);
            var service = CreateService();

            // Assert
            var exception = Assert.Throws<SsiZipFolderProcessingException>(() => service.GetNewestArchivesFromFolder());
            Assert.That(exception.Message.Contains("Could not find directory"));
        }

        [Test]
        public void GetNewestArchivesFromFolder_DirectoryContainsNotOnlyZips_ShouldThrowProperException()
        {
            // Arrange
            _sampleListOfFiles[0] = "notzipfile.exe";
            ConfigureMocks();
            var service = CreateService();

            // Assert
            var exception = Assert.Throws<SsiZipFolderProcessingException>(() => service.GetNewestArchivesFromFolder());
            Assert.That(exception.Message.Contains("does not contain only zip"));
        }

        [Test]
        public void GetNewestArchivesFromFolder_CannotParseDateOfSomeInfectionFile_ShouldLogWarning()
        {
            // Arrange
            var expectedDate = new DateTime(2020, 12, 10);
            ZipArchive expectedZipArchiveToday = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[9])).Returns(expectedDate);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[9])).Returns(expectedZipArchiveToday);
            

            var expectedVaccineDate = new DateTime(2020, 12, 10);
            ZipArchive expectedZipArchiveVaccination = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[6])).Returns(expectedVaccineDate);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[6])).Returns(expectedZipArchiveVaccination);
            ConfigureMocks();
            var service = CreateService();

            // Act
            service.GetNewestArchivesFromFolder();

            // Assert
            _mockLogger.Verify(x => x.LogWarning(It.IsRegex("Could not parse date")));
        }

        [Test]
        public void GetNewestArchivesFromFolder_CannotParseDateOfSomeVaccineFile_ShouldLogWarning()
        {
            // Arrange
            var expectedDate = new DateTime(2020, 12, 10);
            ZipArchive expectedZipArchiveToday = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[9])).Returns(expectedDate);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[9])).Returns(expectedZipArchiveToday);

            var expectedVaccineDate = new DateTime(2020, 12, 10);
            ZipArchive expectedZipArchiveVaccination = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[6])).Returns(expectedVaccineDate);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[6])).Returns(expectedZipArchiveVaccination);

            ConfigureMocks();

            var service = CreateService();

            // Act
            service.GetNewestArchivesFromFolder();

            // Assert
            _mockLogger.Verify(x => x.LogWarning(It.IsRegex("Could not parse date")));
        }

        [Test]
        public void GetNewestArchivesFromFolder_ThereAreCorrectFiles_ShouldReturnProperZipArchives()
        {
            // Arrange
            ConfigureMocks();
            var expectedDate = new DateTime(2020, 12, 10);
            var expectedVaccinationDate = new DateTime(2020, 12, 10, 10, 10, 10);

            ZipArchive expectedZipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            ZipArchive expectedZipArchiveVaccination = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);

            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[6])).Returns(expectedVaccinationDate);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[8])).Returns(expectedVaccinationDate);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[9])).Returns(expectedDate);

            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[6])).Returns(expectedZipArchiveVaccination);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[8])).Returns(expectedZipArchiveVaccination);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[9])).Returns(expectedZipArchive);
            
            var service = CreateService();

            // Act
            var result = service.GetNewestArchivesFromFolder();

            // Assert
            Assert.AreEqual(expectedDate, result.DateInfection);
            Assert.AreEqual(expectedZipArchive, result.StatisticsArchive);

            Assert.AreEqual(expectedVaccinationDate, result.DateVaccination);
            Assert.AreEqual(expectedZipArchiveVaccination, result.VaccinationArchive);
        }

        [Test]
        public void GetNewestArchivesFromFolder_NewArchiveExistCannotParseDateOfSomeFiles_ShouldLogWarning()
        {
            // Arrange
            _sampleListOfFiles[5] = "Smittestop_smittetal_InvalidDateInFileName.zip";

            var expectedDate = new DateTime(2020, 12, 10);
            ZipArchive expectedZipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            ZipArchive expectedZipArchiveYesterday = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[6])).Returns(expectedDate);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[8])).Returns(expectedDate);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[6])).Returns(expectedZipArchive);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[8])).Returns(expectedZipArchive);

            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[9])).Returns(expectedDate);

            ZipArchive expectedZipArchiveVaccination = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[9])).Returns(expectedZipArchiveVaccination);

            ConfigureMocks();

            var service = CreateService();

            // Act
            service.GetNewestArchivesFromFolder();

            // Assert
            _mockLogger.Verify(x => x.LogWarning(It.IsRegex("Could not parse date")));
        }

        [Test]
        public void GetNewestArchivesFromFolder_NewArchiveExistCannotParseDateOfSomeVaccinationFile_ShouldLogWarning()
        {
            // Arrange
            ConfigureMocks();
            var expectedDate = new DateTime(2020, 12, 10);
            var expectedVaccinationDate = new DateTime(2020, 12, 10, 10, 10, 10);

            ZipArchive expectedZipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            ZipArchive expectedZipArchiveVaccination = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);

            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[6])).Returns(expectedVaccinationDate);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[8])).Returns(expectedVaccinationDate);
            _mockFileSystem.Setup(x => x.GetCreationDateUTC(_sampleListOfFiles[9])).Returns(expectedDate);

            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[6])).Returns(expectedZipArchiveVaccination);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[8])).Returns(expectedZipArchiveVaccination);
            _mockFileSystem.Setup(x => x.OpenZip(_sampleListOfFiles[9])).Returns(expectedZipArchive);

            var service = CreateService();

            // Act
            service.GetNewestArchivesFromFolder();

            // Assert
            _mockLogger.Verify(x => x.LogWarning(It.IsRegex("Could not parse date")));
        }
    }
}
