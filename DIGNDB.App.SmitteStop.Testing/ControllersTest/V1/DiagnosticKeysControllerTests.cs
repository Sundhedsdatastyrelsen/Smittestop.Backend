using DIGNDB.App.SmitteStop.API;
using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using DIGNDB.App.SmitteStop.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Testing.ControllersTest.V1
{

    [TestFixture]
    public class DiagnosticKeysControllerTests
    {
        private Mock<IAppleService> _appleService;
        private Mock<ICacheOperations> _cacheOperation;
        private Mock<ITemporaryExposureKeyRepository> _temporaryExposureKeyRepository;
        private Mock<IConfiguration> _configuration;
        private Mock<IExposureKeyValidator> _exposureKeyValidator;
        private Mock<ILogger<DiagnosticKeysController>> _logger;
        private Mock<IExposureConfigurationService> _exposureConfigurationService;
        private Mock<IKeyValidationConfigurationService> _keyValidationConfigurationService;
        private Mock<IExportKeyConfigurationService> _exportKeyConfigurationService;
        private Mock<ICountryRepository> _countryRepository;
        private Mock<IAppSettingsConfig> _appSettingsConfigMock;
        private Mock<IAddTemporaryExposureKeyService> _addTemporaryExposureKeyService;
        private DiagnosticKeysController _controller;

        private readonly List<TemporaryExposureKey> _exampleKeys = new List<TemporaryExposureKey>()
        {
            new TemporaryExposureKey(),
            new TemporaryExposureKey(),
            new TemporaryExposureKey(),
        };

        [SetUp]
        public void Init()
        {
            SetupMockServices();
            _controller = new DiagnosticKeysController(_cacheOperation.Object, _logger.Object, _appleService.Object, 
                _temporaryExposureKeyRepository.Object, _configuration.Object, _exposureKeyValidator.Object, 
                _exposureConfigurationService.Object, _keyValidationConfigurationService.Object, 
                _countryRepository.Object, _appSettingsConfigMock.Object, _addTemporaryExposureKeyService.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = MakeFakeContext(true).Object }
            };
        }

        private void SetupMockServices()
        {
            _logger = new Mock<ILogger<DiagnosticKeysController>>();
            _appleService = new Mock<IAppleService>(MockBehavior.Strict);
            _temporaryExposureKeyRepository = new Mock<ITemporaryExposureKeyRepository>();
            _configuration = new Mock<IConfiguration>();
            
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("false");
            _configuration.Setup(a => a.GetSection("deviceVerificationEnabled")).Returns(configurationSection.Object);

            _exposureKeyValidator = new Mock<IExposureKeyValidator>();
            _addTemporaryExposureKeyService = new Mock<IAddTemporaryExposureKeyService>(MockBehavior.Strict);
            _exposureConfigurationService = new Mock<IExposureConfigurationService>(MockBehavior.Strict);
            _keyValidationConfigurationService = new Mock<IKeyValidationConfigurationService>();
            _cacheOperation = new Mock<ICacheOperations>(MockBehavior.Strict);
            _exportKeyConfigurationService = new Mock<IExportKeyConfigurationService>(MockBehavior.Strict);
            _countryRepository = new Mock<ICountryRepository>();
            _appSettingsConfigMock = new Mock<IAppSettingsConfig>();
            _appSettingsConfigMock.Setup(mock => mock.Configuration)
                .Returns(_configuration.Object);
            _addTemporaryExposureKeyService.Setup(x => x.GetFilteredKeysEntitiesFromDTO(It.IsAny<TemporaryExposureKeyBatchDto>()))
                .ReturnsAsync(new List<TemporaryExposureKey>()
                {
                    new TemporaryExposureKey()
                });
            SetupMockConfiguration();
            SetupMockExposureConfigurationService();
            setupMockExportKeyConfigurationService();
            SetupMockCacheOperation(1);
        }

        private void SetupMockConfiguration()
        {
            _configuration.Setup(config => config["AppSettings:deviceVerificationEnabled"]).Returns("false");
            _configuration.Setup(config => config["KeyValidationRules:PackageNames:ios"]).Returns("com.netcompany.smittestop-exposure-notification");
            _configuration.Setup(config => config["KeyValidationRules:PackageNames:android"]).Returns("com.netcompany.smittestop_exposure_notification");
            _configuration.Setup(config => config["AppSettings:CacheMonitorTimeout"]).Returns("100");
            _configuration.Setup(config => config["AppSettings:fetchCommandTimeout"]).Returns("0");
            _configuration.Setup(config => config["AppSettings:enableCacheOverride"]).Returns("true");
        }

        private void SetupMockExposureConfigurationService()
        {
            _exposureConfigurationService.Setup(s => s.GetConfiguration())
                .Returns(Task.FromResult(new ExposureConfiguration()));
        }

        private List<byte[]> mockCacheResult(int count)
        {
            List<byte[]> files = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                files.Add(Encoding.UTF8.GetBytes("file" + i));
            }
            return files;
        }

        private void SetupMockCacheOperation(int count)
        {
            _cacheOperation.Setup(c => c.GetCacheValue(It.IsAny<DateTime>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new CacheResult()
                {
                    FileBytesList = mockCacheResult(count)
                }));
        }

        private void setupMockExportKeyConfigurationService()
        {
            _exportKeyConfigurationService.Setup(k => k.GetConfiguration()).Returns(new ExportKeyConfiguration()
            {
                CurrentDayFileCaching = TimeSpan.Parse("02:00:00.000"),
                PreviousDayFileCaching = TimeSpan.Parse("15.00:00:00.000"),
                MaxKeysPerFile = 750000
            });
        }

        public static IEnumerable<string> InvalidDate
        {
            get
            {
                yield return DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd");
                yield return DateTime.UtcNow.AddDays(-15).ToString("yyyy'-'MM'-'dd");
            }
        }

        public Mock<HttpContext> MakeFakeContext(bool hasCacheControl, bool emptyBody = true)
        {
            var mockRequest = new Mock<HttpRequest>();
            var requestHeader = new Mock<HeaderDictionary>();
            var mockResponse = new Mock<HttpResponse>();
            var responseHeader = new Mock<HeaderDictionary>();
            if (hasCacheControl)
            {
                requestHeader.Object.Add("Cache-Control", "no-cache");
            }
            mockRequest.Setup(res => res.Headers).Returns(requestHeader.Object);

            if (!emptyBody)
            {
                var ms = new MemoryStream();
                var bytes = JsonSerializer.SerializeToUtf8Bytes(new TemporaryExposureKeyBatchDto()
                {
                    keys = new List<TemporaryExposureKeyDto>
                    {
                        new TemporaryExposureKeyDto() { rollingStart = DateTime.Now},
                        new TemporaryExposureKeyDto() { rollingStart = DateTime.Now},
                        new TemporaryExposureKeyDto() { rollingStart = DateTime.Now},
                    },
                    regions = new List<string> { "DK" }
                });
                ms.Write(bytes);
                ms.Seek(0, SeekOrigin.Begin);
                mockRequest.Setup(res => res.Body).Returns(ms);
            }

            mockResponse.Setup(res => res.Headers).Returns(responseHeader.Object);
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);
            return mockContext;
        }

        [Test]
        public void GetExposureConfiguration_ShouldReturnOkResult()
        {
            var result = _controller.GetExposureConfiguration(true, new CancellationToken());
            Assert.That(result.Result, Is.InstanceOf<StatusCodeResult>());
        }

        [Test]
        public void UploadDiagnosisKeys_ShouldReturnBadResultResultDueToInvalidBody()
        {
            var result = _controller.UploadDiagnosisKeys();
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void UploadDiagnosisKeys_ShouldSaveKeySourceAsVersion1IfControllerInVersion1()
        {
            _controller.ControllerContext = new ControllerContext
            { HttpContext = MakeFakeContext(true, false).Object };
            var result = _controller.UploadDiagnosisKeys();

            //_temporaryExposureKeyRepository.Verify(mock =>
            //    mock.AddTemporaryExposureKeys(It.Is<IList<TemporaryExposureKey>>(keys =>
            //        keys.All(key => key.KeySource == KeySource.SmitteStopApiVersion1))));
        }

        [Test]
        [TestCaseSource(nameof(InvalidDate))]
        public void DownloadDiagnosisKeysFile_GiveInvalidDate_ShouldReturnBadRequest(string invalidDate)
        {
            var result = _controller.DownloadDiagnosisKeysFile(invalidDate);
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void DownloadDiagnosisKeysFile_GiveInvalidFormatPackageName_ShouldReturnInternalServerError()
        {
            var invalidPackageName = "abc";
            var result = _controller.DownloadDiagnosisKeysFile(invalidPackageName);
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void DownloadDiagnosisKeysFile_CreateFileInDemand_ShouldReturnZipStream()
        {
            var expectDate = DateTime.UtcNow;
            var packageName = expectDate.ToString("yyyy'-'MM'-'dd");

            var result = _controller.DownloadDiagnosisKeysFile(packageName);

            Assert.That(((FileContentResult)result.Result).FileContents, Is.Not.Empty);
            Assert.That(((FileContentResult)result.Result).ContentType, Is.EqualTo("application/zip"));
            Assert.That(_controller.Response.Headers["Batchcount"].ToString(), Is.EqualTo("1"));
            Assert.That(bool.Parse(_controller.Response.Headers["FinalForTheDay"].ToString()), Is.False);
        }

        [Test]
        public void DownloadDiagnosisKeysFile_CreateFile_ShouldReturnZipStream()
        {
            var expectDate = DateTime.UtcNow.AddDays(-1);
            var packageName = expectDate.ToString("yyyy'-'MM'-'dd");

            //remove Cache-Control header to make sure the test covers caching feature
            var contextWithoutCacheControl = MakeFakeContext(false).Object;
            _controller.ControllerContext.HttpContext = contextWithoutCacheControl;
            var result = _controller.DownloadDiagnosisKeysFile(packageName);

            Assert.That(((FileContentResult)result.Result).FileContents, Is.Not.Empty);
            Assert.That(((FileContentResult)result.Result).ContentType, Is.EqualTo("application/zip"));
            Assert.That(_controller.Response.Headers["Batchcount"].ToString(), Is.EqualTo("1"));
            Assert.That(bool.Parse(_controller.Response.Headers["FinalForTheDay"].ToString()), Is.True);
        }

        [Test]
        public void DownloadDiagnosisKeysFile_CreateFileWithNoKey_ShouldReturnNoContentStatusCode()
        {
            var expectDate = DateTime.UtcNow.AddDays(-1);
            var packageName = expectDate.ToString("yyyy'-'MM'-'dd");
            //set mock cache result to return empty file content
            SetupMockCacheOperation(0);
            //remove Cache-Control header to make sure the test covers caching feature
            var contextWithoutCacheControl = MakeFakeContext(false).Object;
            _controller.ControllerContext.HttpContext = contextWithoutCacheControl;
            var result = _controller.DownloadDiagnosisKeysFile(packageName);

            Assert.That(((StatusCodeResult)result.Result).StatusCode, Is.EqualTo(204));
        }

        [Test]
        public void ReturnBadRequest_When_RequestBodyIsNotParsable()
        {
            //Arrange
            var badRequestJsonBodyStream = new MemoryStream(Encoding.UTF8.GetBytes("attribute :**? value"));
            var contextWithoutCacheControl = MakeFakeContext(false);
            contextWithoutCacheControl.Setup(x => x.Request.Body).Returns(badRequestJsonBodyStream);
            contextWithoutCacheControl.Setup(x => x.Request.ContentLength).Returns(badRequestJsonBodyStream.Length);
            _controller.ControllerContext.HttpContext = contextWithoutCacheControl.Object;

            //Act
            var result = _controller.UploadDiagnosisKeys().Result;

            //Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            contextWithoutCacheControl.Verify(c => c.Request.Body, Times.Once);
            Assert.That(((BadRequestObjectResult)result).Value.ToString(), Does.StartWith("Incorrect JSON format:"));
        }

    }
}
