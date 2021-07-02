using DIGNDB.App.SmitteStop.Core.Contracts;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DIGNDB.App.SmitteStop.API.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class HealthCheckHangFireService : IHealthCheckHangFireService
    {
        private readonly IConfiguration _configuration;

        private IMonitoringApi _hangFireMonitoringApi;
        private static JobStorage _jobStorageCurrent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public HealthCheckHangFireService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMonitoringApi GetHangFireMonitoringApi()
        {
            if (_jobStorageCurrent == null)
            {
                InitializeHangFire();
            }

            if (_jobStorageCurrent != null)
            {
                _hangFireMonitoringApi = _jobStorageCurrent.GetMonitoringApi();
            }

            return _hangFireMonitoringApi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<RecurringJobDto> GetRecurringJobs()
        {
            var recurringJobs = _jobStorageCurrent.GetConnection().GetRecurringJobs();
            return recurringJobs;
        }

        private void InitializeHangFire()
        {
            var connectionString = _configuration["HangFireConnectionString"];
            JobStorage.Current = new SqlServerStorage(connectionString);
            _jobStorageCurrent = JobStorage.Current;
        }
    }
}
