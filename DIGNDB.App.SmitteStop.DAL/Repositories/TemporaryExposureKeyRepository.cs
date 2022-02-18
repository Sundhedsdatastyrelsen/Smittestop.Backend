using DIGNDB.App.SmitteStop.DAL.Context;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Profiling;
using SortOrder = DIGNDB.App.SmitteStop.Domain.SortOrder;

namespace DIGNDB.App.SmitteStop.DAL.Repositories
{
    public class TemporaryExposureKeyRepository : ITemporaryExposureKeyRepository
    {
        private readonly DigNDB_SmittestopContext _dbContext;
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<ITemporaryExposureKeyRepository> _logger;

        // constructor used for unit tests
        public TemporaryExposureKeyRepository(DigNDB_SmittestopContext dbContext, ICountryRepository countryRepository, ILogger<ITemporaryExposureKeyRepository> logger)
        {
            _countryRepository = countryRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task AddTemporaryExposureKey(TemporaryExposureKey temporaryExposureKey)
        {
            using (MiniProfiler.Current.Step("Repo/ExposureKey/Add"))
            {
                _dbContext.TemporaryExposureKey.Add(temporaryExposureKey);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task AddTemporaryExposureKeys(IList<TemporaryExposureKey> temporaryExposureKeys)
        {
            using (MiniProfiler.Current.Step("Repo/ExposureKey/AddList"))
            {
                foreach (var key in temporaryExposureKeys)
                {
                    await _dbContext.AddAsync(key);
                }

                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IList<TemporaryExposureKey>> GetAll()
        {
            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetAll"))
            {
                return await _dbContext.TemporaryExposureKey.ToListAsync();
            }
        }

        public async Task<IList<byte[]>> GetAllKeyData()
        {
            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetAllKeys"))
            {
                return await _dbContext.TemporaryExposureKey.Select(x => x.KeyData).ToListAsync();
            }
        }

        public async Task<byte[][]> GetKeysThatAlreadyExistsInDbAsync(byte[][] incomingKeys)
        {
            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetAlreadyExists"))
            {
                return await _dbContext.TemporaryExposureKey.Where(u => incomingKeys.Contains(u.KeyData))
                    .Select(u => u.KeyData).ToArrayAsync();
            }
        }

        public int GetCountOfKeysByUploadedDayAndSource(DateTime uploadDate, KeySource keySource)
        {
            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetCount"))
            {
                return _dbContext.TemporaryExposureKey
                    .Where(key => key.CreatedOn.Date.CompareTo(uploadDate.Date) == 0)
                    .Count(key => key.KeySource == keySource);
            }
        }

        public IList<TemporaryExposureKey> GetAllKeysNextBatch(int numberOfRecordsToSkip, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentException($"Incorrect argument batchSize= {batchSize}");
            }

            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetAllKeysB"))
            {
                var query = _dbContext.TemporaryExposureKey
                    .OrderBy(c => c.CreatedOn)
                    .Skip(numberOfRecordsToSkip)
                    .Take(batchSize);
                return query.ToList();
            }
        }

        public IList<TemporaryExposureKey> GetAllKeysNextBatchWithOriginId(int numberOfRecordsToSkip, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentException($"Incorrect argument batchSize= {batchSize}");
            }

            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetAllBwO"))
            {
                var query = _dbContext.TemporaryExposureKey
                    .OrderBy(c => c.CreatedOn)
                    .Skip(numberOfRecordsToSkip).Include(x => x.Origin)
                    .Take(batchSize).ToList();
                return query;
            }
        }

        public async Task<TemporaryExposureKey> GetById(Guid id)
        {
            return await _dbContext.TemporaryExposureKey.FirstOrDefaultAsync(x => x.Id == id);
        }

        public IList<TemporaryExposureKey> GetTemporaryExposureKeysWithDkOrigin(DateTime uploadedOn, int fetchTimeout)
        {
            var dkCountry = _countryRepository.GetDenmarkCountry();
            if (fetchTimeout > 0)
            {
                _dbContext.Database.SetCommandTimeout(fetchTimeout);
            }

            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetTemporaryExposureKeysWithDkOrigin"))
            {
                return _dbContext.TemporaryExposureKey
                    .Where(x => x.Origin == dkCountry && x.CreatedOn.Date.CompareTo(uploadedOn.Date) == 0)
                    .OrderBy(x => x.Id).ToList();
            }
        }

        public IList<TemporaryExposureKey> GetDkTemporaryExposureKeysUploadedAfterTheDateForGatewayUpload(
            DateTime uploadedOnAndLater,
            int numberOfRecordToSkip,
            int maxCount,
            KeySource[] sources,
            bool logInformationKeyValueOnUpload = false)
        {
            if (maxCount <= 0)
            {
                throw new ArgumentException($"Incorrect argument maxCount= {maxCount}");
            }

            var dkCountry = _countryRepository.GetDenmarkCountry();

            LogKeysInformation(_dbContext.TemporaryExposureKey, logInformationKeyValueOnUpload);
            using (MiniProfiler.Current.Step("Repo/ExposureKey/GetTemporaryExposureKeysWithDkOrigin"))
            {
                var query = _dbContext.TemporaryExposureKey
                    .Include(k => k.Origin)
                    .Where(k => k.Origin == dkCountry)
                    .Where(k => k.CreatedOn >= uploadedOnAndLater)
                    .Where(k => sources.Contains(k.KeySource))
                    .Include(k => k.VisitedCountries)
                    .ThenInclude(k => k.Country)
                    .OrderBy(c => c.CreatedOn)
                    .ThenBy(c => c.RollingStartNumber);

                if (logInformationKeyValueOnUpload)
                {
                    _logger.LogInformation($"query.count : {query.Count()}");
                    foreach (var key in query)
                    {
                        var hexValue = ConvertToHexValue(key.KeyData);
                        _logger.LogInformation($"{hexValue} : {key.KeyData}");
                    }
                }


                var retVal = TakeNextBatch(query, numberOfRecordToSkip, maxCount);
                var list = retVal.ToList();


                if (logInformationKeyValueOnUpload)
                {
                    _logger.LogInformation($"List count : {list.Count} : {numberOfRecordToSkip}, {maxCount}");
                }

                LogKeysInformationList(list, dkCountry, uploadedOnAndLater, logInformationKeyValueOnUpload);

                return list;
            }
        }
        
        public async Task<IList<TemporaryExposureKey>> GetNextBatchOfKeysWithRollingStartNumberThresholdAsync(long rollingStartNumberThreshold, int numberOfRecordsToSkip, int batchSize)
        {
            var query = _dbContext.TemporaryExposureKey
               .Include(k => k.Origin)
               .Where(k => k.RollingStartNumber >= rollingStartNumberThreshold);

            query = query.OrderBy(c => c.CreatedOn);
            return await TakeNextBatch(query, numberOfRecordsToSkip, batchSize).ToListAsync();
        }

        private void LogKeysInformation(DbSet<TemporaryExposureKey> temporaryExposureKeys, bool logInformationKeyValueOnUpload = false)
        {
            _logger.LogInformation($"No. of keys in DbSet prior to selecting for upload to EFGS: {temporaryExposureKeys.Count()}");

            foreach (var temporaryExposureKey in temporaryExposureKeys)
            {
                var hexValue = ConvertToHexValue(temporaryExposureKey.KeyData);
                if (logInformationKeyValueOnUpload)
                {
                    //_logger.LogInformation($"Upload to EFGS key.KeyData: {hexValue}");
                }
            }
        }

        private void LogKeysInformationList(List<TemporaryExposureKey> temporaryExposureKeys, Country dkCountry,
            DateTime uploadedOnAndLater, bool logInformationKeyValueOnUpload = false)
        {
            _logger.LogInformation($"No. of keys uploaded to EFGS: {temporaryExposureKeys.Count}, {uploadedOnAndLater} , {dkCountry.Code}");

            foreach (var temporaryExposureKey in temporaryExposureKeys)
            {
                var hexValue = ConvertToHexValue(temporaryExposureKey.KeyData);
                if (logInformationKeyValueOnUpload)
                {
                    var logMessage = $"EFGS upload selected key: {hexValue}, {dkCountry.Code}, ${uploadedOnAndLater}";
                    _logger.LogInformation(logMessage);
                }
            }
        }

        private string ConvertToHexValue(byte[] keyData)
        {
            var hexValue = BitConverter.ToString(keyData);
            hexValue = hexValue.Replace("-", "");
            return hexValue;
        }

        #region Query based on CreatedOn

        public IList<TemporaryExposureKey> GetDkTemporaryExposureKeysForPeriodNextBatch(DateTime uploadedAfter, int numberOfRecordsToSkip, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentException($"Incorrect argument batchSize= {batchSize}");
            }

            var query = CreateQueryForKeysUploadedAfterTheDate(uploadedAfter, SortOrder.ASC);
            var dkCountry = _countryRepository.GetDenmarkCountry();
            query = query.Where(x => x.Origin == dkCountry);

            return TakeNextBatch(query, numberOfRecordsToSkip, batchSize).ToList();
        }

        public IList<TemporaryExposureKey> GetAllTemporaryExposureKeysForPeriodNextBatch(DateTime uploadedAfter, int numberOfRecordsToSkip, int batchSize)
        {
            var retVal = GetAllTemporaryExposureKeysForPeriodNextBatchQuery(uploadedAfter, numberOfRecordsToSkip, batchSize).ToList();
            return retVal;
        }

        public IQueryable<TemporaryExposureKey> GetAllTemporaryExposureKeysForPeriodNextBatchQuery(DateTime uploadedAfter, int numberOfRecordsToSkip, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentException($"Incorrect argument batchSize= {batchSize}");
            }

            var query = CreateQueryForKeysUploadedAfterTheDate(uploadedAfter, SortOrder.ASC);

            return TakeNextBatch(query, numberOfRecordsToSkip, batchSize);
        }

        private IQueryable<TemporaryExposureKey> CreateQueryForKeysUploadedAfterTheDate(DateTime uploadedAfter, SortOrder sortOrder)
        {
            var query = _dbContext.TemporaryExposureKey
               .Include(k => k.Origin)
               .Where(k => k.CreatedOn > uploadedAfter);

            query = sortOrder == SortOrder.ASC ? query.OrderBy(c => c.CreatedOn) : query.OrderByDescending(c => c.CreatedOn);
            return query;
        }

        #endregion

        private IQueryable<TemporaryExposureKey> TakeNextBatch(IQueryable<TemporaryExposureKey> keys, int numberOfRecordsToSkip, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentException($"Incorrect argument batchSize= {batchSize}");
            }

            _logger.LogInformation($"Upload to EFGS key.KeyData: {numberOfRecordsToSkip} : {batchSize}");

            return keys.Skip(numberOfRecordsToSkip).Take(batchSize);
        }

        public void RemoveKeys(List<TemporaryExposureKey> keys)
        {
            _dbContext.RemoveRange(keys);
            _dbContext.SaveChanges();
        }

        public void UpdateKeysRollingStartField(List<TemporaryExposureKey> keys)
        {
            _dbContext.UpdateRange(keys);
            _dbContext.SaveChanges();
        }
    }
}
