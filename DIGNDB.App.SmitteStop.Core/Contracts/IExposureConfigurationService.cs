using DIGNDB.App.SmitteStop.Core.Models;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Core.Contracts
{
    public interface IExposureConfigurationService
    {
        void SetConfiguration(IConfiguration configuration);
        Task<ExposureConfiguration> GetConfiguration();
        Task<ExposureConfigurationV1_2> GetConfigurationR1_2();
        Task<DailySummaryExposureConfiguration> GetDailySummaryConfiguration();
    }
}
