using DIGNDB.App.SmitteStop.DAL.Context;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using DIGNDB.App.SmitteStop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIGNDB.App.SmitteStop.Testing.RepositoriesTest
{
    [TestFixture]
    public class TemporaryExposureKeyRepositoryTests
    {
        private MockRepository _mockRepository;

        private TemporaryExposureKeyRepository _repo;
        private DbContextOptions<DigNDB_SmittestopContext> _options;
        private Mock<ICountryRepository> _countryRepository;
        private Mock<ILogger<ITemporaryExposureKeyRepository>> _mockLogger;

        private readonly Country _dkCountry = new Country
        {
            Id = 1,
            Code = "dk"
        };

        private readonly Country _notDkCountry = new Country
        {
            Id = 2,
            Code = "not dk"
        };

        [SetUp]
        public void CreateOptions()
        {
            var DBName = "TEST_DB_" + DateTime.UtcNow;
            _options = new DbContextOptionsBuilder<DigNDB_SmittestopContext>().UseInMemoryDatabase(databaseName: DBName).Options;
            _countryRepository = new Mock<ICountryRepository>(MockBehavior.Strict);
            _countryRepository.Setup(x => x.GetDenmarkCountry()).Returns(_dkCountry);

            _mockRepository = new MockRepository(MockBehavior.Loose);
            _mockLogger = _mockRepository.Create<ILogger<ITemporaryExposureKeyRepository>>();
        }

        private IList<TemporaryExposureKey> CreateMockedListExposureKeys(DateTime expectDate, int numberOfKeys, bool isDkOrigin, List<TemporaryExposureKeyCountry> visitedCountries = null)
        {
            List<TemporaryExposureKey> data = new List<TemporaryExposureKey>();
            for (int i = 0; i < numberOfKeys; i++)
            {
                data.Add(new TemporaryExposureKey()
                {
                    CreatedOn = expectDate.Date,
                    Id = Guid.NewGuid(),
                    KeyData = Encoding.ASCII.GetBytes("keyData" + (i + 1)),
                    TransmissionRiskLevel = RiskLevel.RISK_LEVEL_LOW,
                    KeySource = KeySource.SmitteStopApiVersion2,
                    Origin = _dkCountry
                });

                if (!isDkOrigin)
                {
                    data.Last().Origin = _notDkCountry;
                }

                if (visitedCountries == null || !visitedCountries.Any())
                {
                    continue;
                }

                foreach (var temporaryExposureKey in data)
                {
                    temporaryExposureKey.VisitedCountries = visitedCountries;
                }
            }
            return data;
        }

        [Test]
        public void GetTemporaryExposureKeys_HaveRecord_ShouldReturnCorrectRecordMatchedRequirement()
        {
            var expectDate = DateTime.UtcNow;
            var dataForCurrentDate = CreateMockedListExposureKeys(expectDate, 2, true);
            var dataForOtherDate = CreateMockedListExposureKeys(expectDate.AddDays(-12), 3, true);
            var dataForNotDK = CreateMockedListExposureKeys(expectDate.AddDays(-12), 3, false);
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                context.Database.EnsureDeleted();
                //add data to context
                context.TemporaryExposureKey.AddRange(dataForCurrentDate);
                context.TemporaryExposureKey.AddRange(dataForOtherDate);
                context.TemporaryExposureKey.AddRange(dataForNotDK);
                context.SaveChanges();
                _repo = new TemporaryExposureKeyRepository(context, _countryRepository.Object, _mockLogger.Object);
                var keys = _repo.GetTemporaryExposureKeysWithDkOrigin(expectDate, 0);
                Assert.AreEqual(dataForCurrentDate.Count, keys.Count);
            }
        }

        [Test]
        public void GetById_HaveRecord_ShouldReturnCorrectRecord()
        {
            var data = CreateMockedListExposureKeys(DateTime.UtcNow, 4, true);
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                context.Database.EnsureDeleted();
                context.TemporaryExposureKey.AddRange(data);
                context.SaveChanges();
                _repo = new TemporaryExposureKeyRepository(context, _countryRepository.Object, _mockLogger.Object);
                var expectKey = data[0];
                var actualKey = _repo.GetById(expectKey.Id).Result;

                Assert.AreEqual(expectKey.Id, actualKey.Id);
                Assert.AreEqual(expectKey.TransmissionRiskLevel, actualKey.TransmissionRiskLevel);
                Assert.AreEqual(expectKey.CreatedOn, actualKey.CreatedOn);
                Assert.AreEqual(expectKey.KeyData, actualKey.KeyData);
            }
        }

        [Test]
        public void GetAll_HaveData_ShouldReturnCorrectNumberOfRecord()
        {
            var expectKeys = 4;
            var data = CreateMockedListExposureKeys(DateTime.UtcNow, expectKeys, true);
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                context.Database.EnsureDeleted();
                //add data to context
                context.TemporaryExposureKey.AddRange(data);
                context.SaveChanges();
                _repo = new TemporaryExposureKeyRepository(context, _countryRepository.Object, _mockLogger.Object);
                var keys = _repo.GetAll().Result;
                Assert.AreEqual(expectKeys, keys.Count);
            }
        }

        [Test]
        public void GetAllKeyData_HaveData_ShouldReturnCorrectResult()
        {
            var data = CreateMockedListExposureKeys(DateTime.UtcNow, 4, true);
            var expectKeysData = data.Select(x => x.KeyData).ToList();
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                context.Database.EnsureDeleted();
                //add data to context
                context.TemporaryExposureKey.AddRange(data);
                context.SaveChanges();
                _repo = new TemporaryExposureKeyRepository(context, _countryRepository.Object, _mockLogger.Object);
                var keys = _repo.GetAllKeyData().Result;
                CollectionAssert.AreEqual(expectKeysData, keys);
            }
        }

        [Test]
        public void AddTemporaryExposureKey_ProvideKey_ShouldAddNewKeyToDB()
        {
            TemporaryExposureKey key = new TemporaryExposureKey()
            {
                CreatedOn = DateTime.UtcNow.Date,
                Id = Guid.NewGuid(),
                KeyData = Encoding.ASCII.GetBytes("keyData"),
                TransmissionRiskLevel = RiskLevel.RISK_LEVEL_LOW,
            };
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                context.Database.EnsureDeleted();
                _repo = new TemporaryExposureKeyRepository(context, _countryRepository.Object, _mockLogger.Object);
                _repo.AddTemporaryExposureKey(key).Wait();
            }
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                var keyInDB = context.TemporaryExposureKey.ToList();
                Assert.AreEqual(1, keyInDB.Count);
                Assert.IsNotNull(keyInDB.FirstOrDefault(k => k.Id == key.Id));
            }
        }

        // This test needs to be fixed however I don't know how to do that :(
        [Test]
        public void AddTemporaryExposureKeys_ProvideKeys_ShouldAddNewKeysToDB()
        {
            var data = CreateMockedListExposureKeys(DateTime.UtcNow, 4, false);
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                context.Database.EnsureDeleted();
                _repo = new TemporaryExposureKeyRepository(context, _countryRepository.Object, _mockLogger.Object);
                _repo.AddTemporaryExposureKeys(data).Wait();
            }
            using (var context = new DigNDB_SmittestopContext(_options))
            {
                var keyInDB = context.TemporaryExposureKey.ToList();
                Assert.AreEqual(4, keyInDB.Count);
            }
        }

        [Test]
        public void GetDkTemporaryExposureKeysUploadedAfterTheDateForGatewayUpload_HasVisitedCountriesChecked_ReturnsCorrectResult()
        {
            // Arrange
            const int numberOfKeys = 5;
            var data = CreateMockedListExposureKeys(DateTime.UtcNow, numberOfKeys, true);
            using var context = new DigNDB_SmittestopContext(_options);
            context.Database.EnsureDeleted();
            context.TemporaryExposureKey.AddRange(data);
            context.SaveChanges();

            _repo = new TemporaryExposureKeyRepository(context, _countryRepository.Object, _mockLogger.Object);
            var uploadedOnAndLater = DateTime.Now.AddDays(-1);
            var sources = new[] {KeySource.SmitteStopApiVersion2};

            AddVisitedCountries(data);

            // Act
            var keysToGateway = _repo.GetDkTemporaryExposureKeysUploadedAfterTheDateForGatewayUpload(uploadedOnAndLater, 0, 10, sources);
            
            // Assert
            foreach (var key in keysToGateway)
            {
                var visited = key.VisitedCountries.ToList();
                foreach (var country in visited)
                {
                    Assert.IsNotNull(country);
                }
                
                Assert.AreEqual(visited.Count, numberOfKeys);
            }
        }

        private void AddVisitedCountries(IList<TemporaryExposureKey> temporaryExposureKeys)
        {
            var countries = GetCountries(temporaryExposureKeys.Count);

            foreach (var key in temporaryExposureKeys)
            {
                var vcs = TemporaryExposureKeyCountries(temporaryExposureKeys, countries);

                key.VisitedCountries = vcs;
            }
        }

        private static List<TemporaryExposureKeyCountry> TemporaryExposureKeyCountries(IList<TemporaryExposureKey> temporaryExposureKeys, List<Country> countries)
        {
            var vcs = new List<TemporaryExposureKeyCountry>();

            for (var i = 0; i < countries.Count; i++)
            {
                var next = countries[i];

                var tempCountry = new TemporaryExposureKeyCountry
                {
                    Country = next,
                    CountryId = next.Id,
                    TemporaryExposureKey = temporaryExposureKeys[i],
                    TemporaryExposureKeyId = temporaryExposureKeys[i].Id
                };

                vcs.Add(tempCountry);
            }

            return vcs;
        }

        private List<Country> GetCountries(int number)
        {
            var retVal = new List<Country>();
            var rand = new Random();
            for (var i = 0; i < number; i++)
            {
                var nextIndex = rand.Next(0, _countries.Length);
                var next = _countries[nextIndex];
                retVal.Add(next);
            }

            return retVal;
        }

        private Country[] _countries =
        {
            new Country {Id = 1, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "AT"},
            new Country {Id = 2, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "BE"},
            new Country {Id = 3, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "BG"},
            new Country {Id = 4, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "HR"},
            new Country {Id = 5, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "CY"},
            new Country {Id = 6, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "CZ"},
            new Country {Id = 8, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "EE"},
            new Country {Id = 9, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "FI"},
            new Country {Id = 10, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "FR"},
            new Country {Id = 11, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "DE"},
            new Country {Id = 12, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "GR"},
            new Country {Id = 13, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "HU"},
            new Country {Id = 14, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "IE"},
            new Country {Id = 15, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "IT"},
            new Country {Id = 16, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "LV"},
            new Country {Id = 17, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "LT"},
            new Country {Id = 18, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "LU"},
            new Country {Id = 19, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "MT"},
            new Country {Id = 20, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "NL"},
            new Country {Id = 21, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "PL"},
            new Country {Id = 22, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "PT"},
            new Country {Id = 23, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "RO"},
            new Country {Id = 24, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "SK"},
            new Country {Id = 25, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "SI"},
            new Country {Id = 26, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "ES"},
            new Country {Id = 27, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "SE"},
            new Country {Id = 29, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "NO"},

            //new Country {Id = 7L, PullingFromGatewayEnabled = false, VisitedCountriesEnabled = false, Code = "DK"},
            new Country {Id = 28L, PullingFromGatewayEnabled = false, VisitedCountriesEnabled = false, Code = "EN"},
        };
    }
}
