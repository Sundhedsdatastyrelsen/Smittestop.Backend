using Hangfire.Storage;
using System.Collections.Generic;

namespace DIGNDB.App.SmitteStop.Core.Contracts
{
    public interface IHealthCheckHangFireService
    {
        public IMonitoringApi GetHangFireMonitoringApi();
        public List<RecurringJobDto> GetRecurringJobs();
    }
}
