using DIGNDB.App.SmitteStop.API.Controllers;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.APP.SmitteStop.API.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Testing.ControllersTest
{
    [TestFixture]
    public class CovidStatisticsUploadControllerTests
    {
        private MockRepository _mockRepository;

        private Mock<ILogger<CovidStatisticsUploadController>> _mockLogger;
        private ApiConfig _mockApiConfig;
        private Mock<IFileSystem> _mockFileSystem;
        private Mock<IUploadFileValidationService> _mockFileValidation;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Loose);

            _mockLogger = _mockRepository.Create<ILogger<CovidStatisticsUploadController>>();
            _mockApiConfig = new ApiConfig
            {
                SSIStatisticsZipFileFolder = "sampleFolder"
            };
            _mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
            _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            _mockFileValidation = new Mock<IUploadFileValidationService>();
            _mockFileValidation.Setup(x => x.Verify(It.IsAny<IFormFile>(), out It.Ref<string>.IsAny)).Returns(true);
        }

        private CovidStatisticsUploadController CreateCovidStatisticsUploadController()
        {
            return new CovidStatisticsUploadController(
                _mockLogger.Object,
                _mockApiConfig,
                _mockFileSystem.Object,
                _mockFileValidation.Object);
        }

        public Mock<HttpContext> MakeFakeContext(int numberOfFiles, bool emptyFiles)
        {
            var mockRequest = new Mock<HttpRequest>();
            var mockedFileCollection = new FormFileCollection();
            string fileContent = "This is a dummy file";
            if (emptyFiles)
            {
                fileContent = "";
            }
            for (int i = 0; i < numberOfFiles; i++)
            {
                mockedFileCollection.Add(new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)), 0, fileContent.Length, "Data", "Smittestop_smittetal_2020_12_15.zip"));
            }
            mockRequest.Setup(c => c.Form).Returns(new FormCollection(new Dictionary<string, StringValues>(), mockedFileCollection));
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            return mockContext;
        }

        public Mock<HttpContext> MakeRealContext_new(int numberOfFiles, bool emptyFiles)
        {
            var mockRequest = new Mock<HttpRequest>();
            var mockedFileCollection = new FormFileCollection();
            string fileContent = "This is a zip file";

            var memoryStream = new MemoryStream();
          
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                if (!emptyFiles)
                {
                    var demoFile = archive.CreateEntry("test1.csv");

                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("Body of CSV file!");
                    }
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < numberOfFiles; i++)
            {
                mockedFileCollection.Add(new FormFile(memoryStream, 0, memoryStream.Length, "Data", "Smittestop_smittetal_2020_12_15.zip"));
            }
            mockRequest.Setup(c => c.Form).Returns(new FormCollection(new Dictionary<string, StringValues>(), mockedFileCollection));
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            return mockContext;
        }

        [Test]
        public async Task UploadCovidStatistics_WrongZipDirectoryPath_ShouldReturn500()
        {
            // Arrange
            _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(false);
            var covidStatisticsUploadController = CreateCovidStatisticsUploadController();

            // Act
            var result = await covidStatisticsUploadController.UploadCovidStatistics();

            // Assert
            Assert.AreEqual(500, ((ObjectResult)result).StatusCode);
        }

        [TestCase(0)]
        [TestCase(2)]
        public async Task UploadCovidStatistics_WrongFilesCount_ShouldReturn400(int numberOfFiles)
        {
            // Arrange
            var mockedContext = MakeFakeContext(numberOfFiles, false);
            var covidStatisticsUploadController = CreateCovidStatisticsUploadController();
            covidStatisticsUploadController.ControllerContext.HttpContext = mockedContext.Object;

            // Act
            var result = await covidStatisticsUploadController.UploadCovidStatistics();

            // Assert
            Assert.AreEqual(400, ((ObjectResult)result).StatusCode);
        }

        [Ignore("Needs validator")]
        [Test]
        public async Task UploadCovidStatistics_EmptyFile_ShouldReturn400()
        {
            // Arrange
            var numberOfFiles = 1;
            var mockedContext = MakeFakeContext(numberOfFiles, true);
            _mockFileSystem.Setup(_ => _.SaveFormFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>())).Throws<Exception>();
            var covidStatisticsUploadController = CreateCovidStatisticsUploadController();
            covidStatisticsUploadController.ControllerContext.HttpContext = mockedContext.Object;

            // Act
            var result = await covidStatisticsUploadController.UploadCovidStatistics();

            // Assert
            Assert.AreEqual(400, ((ObjectResult)result).StatusCode);
        }

        [Test]
        public async Task UploadCovidStatistics_FileCopied_ShouldReturn200()
        {
            // Arrange
            var numberOfFiles = 1;
            var mockedContext = MakeFakeContext(numberOfFiles, false);
            _mockFileSystem.Setup(_ => _.SaveFormFileAsync(It.IsAny<FormFile>(), It.IsAny<string>())).Returns(Task.FromResult(default(object)));
            var covidStatisticsUploadController = CreateCovidStatisticsUploadController();
            covidStatisticsUploadController.ControllerContext.HttpContext = mockedContext.Object;

            // Act
            var result = await covidStatisticsUploadController.UploadCovidStatistics();

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
    }
}
