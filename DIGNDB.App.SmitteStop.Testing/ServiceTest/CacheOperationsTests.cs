using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest
{
    [TestFixture]
    public class CacheOperationsTests
    {
        private Mock<IConfiguration> _configuration;
        private Mock<IPackageBuilderService> _cachePackageBuilder;
        private Mock<ILogger<CacheOperations>> _logger;
        CacheOperations _cacheService;
        MemoryCache _memoryCache;
        private DateTime dateAsKey;

        [SetUp]
        public void init()
        {
            SetupMockConfiguration();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _logger = new Mock<ILogger<CacheOperations>>();
            dateAsKey = DateTime.UtcNow.AddDays(-1).Date;
            _memoryCache.Set(dateAsKey, mockCacheResult);
            _cacheService = new CacheOperations(_memoryCache, _configuration.Object, _cachePackageBuilder.Object, _logger.Object);
        }

        private CacheResult mockCacheResult =>
            new CacheResult() { FileBytesList = new List<byte[]> { Encoding.UTF8.GetBytes("mocked") } };


        private void SetupMockConfiguration()
        {
            _configuration = new Mock<IConfiguration>();
            _configuration.Setup(config => config["AppSettings:CacheMonitorTimeout"]).Returns("100");
            _configuration.Setup(config => config["AppSettings:PreviousDayFileCaching"]).Returns("15.00:00:00.000");
            _configuration.Setup(config => config["AppSettings:CurrentDayFileCaching"]).Returns("02:00:00.000");
            _cachePackageBuilder = new Mock<IPackageBuilderService>();
        }

        [Test]
        public void GetCacheValue_GiveData_ShouldCacheDataAndReturnCorrectCacheResult()
        {
            var expectCacheResult = mockCacheResult;
            var actualCacheResult = _cacheService.GetCacheValue(dateAsKey).Result;

            Assert.IsNotNull(_memoryCache.Get(dateAsKey));
            Assert.IsNotNull(actualCacheResult);
            Assert.IsNotNull(actualCacheResult.FileBytesList);
            CollectionAssert.AreEqual(expectCacheResult.FileBytesList, actualCacheResult.FileBytesList);
        }

    }
}
