using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.APP.SmitteStop.Jobs.Jobs.Interfaces;
using StackExchange.Profiling;

namespace DIGNDB.APP.SmitteStop.Jobs.Jobs
{
    public class CleanupDatabaseJob : ICleanupDatabaseJob
    {
        private readonly IDatabaseKeysValidationService _databaseKeysValidationService;
        public CleanupDatabaseJob(IDatabaseKeysValidationService databaseKeysValidationService)
        {
            _databaseKeysValidationService = databaseKeysValidationService;
        }

        public void ValidateKeysOnDatabase(int batchSize)
        {
            using (MiniProfiler.Current.Step("Job/CleanDatabase/Keys"))
            {
                _databaseKeysValidationService.FindAndRemoveInvalidKeys(batchSize);
            }
        }

        public void ValidateRollingStartOnDatabaseKeys(int batchSize)
        {
            using (MiniProfiler.Current.Step("Job/CleanDatabase/RollingStart"))
            {
                _databaseKeysValidationService.FindInvalidRollingStartEntrysAndUpdateEntry(batchSize);
            }
        }
    }
}
