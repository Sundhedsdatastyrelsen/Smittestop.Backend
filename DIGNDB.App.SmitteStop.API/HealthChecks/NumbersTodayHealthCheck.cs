using DIGNDB.App.SmitteStop.API.HealthCheckAuthorization;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.APP.SmitteStop.API.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API.HealthChecks
{
    /// <summary>
    /// Implementation of IHealthCheck for log files health checks
    /// </summary>
    [Authorize(Policy = HealthCheckBasicAuthenticationHandler.HealthCheckBasicAuthenticationScheme)]
    public class NumbersTodayHealthCheck : TimedHealthCheck, IHealthCheck
    {
        private const string Description = "Numbers today health check inspects presence of and accessibility to files containing number for today including database update";

        private readonly ApiConfig _apiConfiguration;
        private readonly ILogger<NumbersTodayHealthCheck> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISSIStatisticsRepository _ssiIStatisticsRepository;
        private readonly ISSIStatisticsVaccinationRepository _ssiISsiStatisticsVaccinationRepository;

        /// <summary>
        /// Ctor 
        /// </summary>
        /// <param name="apiConfiguration"></param>
        /// <param name="logger"></param>
        /// <param name="fileSystem"></param>
        /// <param name="ssiIStatisticsRepository"></param>
        /// <param name="ssiISsiStatisticsVaccinationRepository"></param>
        public NumbersTodayHealthCheck(ApiConfig apiConfiguration, ILogger<NumbersTodayHealthCheck> logger,
            IFileSystem fileSystem, ISSIStatisticsRepository ssiIStatisticsRepository,
            ISSIStatisticsVaccinationRepository ssiISsiStatisticsVaccinationRepository)
        {
            _apiConfiguration = apiConfiguration;
            _logger = logger;
            _fileSystem = fileSystem;
            _ssiIStatisticsRepository = ssiIStatisticsRepository;
            _ssiISsiStatisticsVaccinationRepository = ssiISsiStatisticsVaccinationRepository;
        }

        /// <summary>
        /// Checks access to log files and that they are written to every day
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation($"Health check {Startup.NumbersTodayPattern}");

            var status = HealthStatus.Healthy;
            var data = new Dictionary<string, object>();

            var hour = _apiConfiguration.HealthCheckSettings.NumbersTodayCallAfter24Hour;
            if (TooEarly(hour, _logger))
            {
                var key = $"Too early to check numbers today {DateTime.Now}";
                data.Add(key, $"Configured value is {hour}");
                return await Task.FromResult(new HealthCheckResult(
                    status,
                    Description,
                    data: data));
            }

            // Check directory exists
            var directoryPath = _apiConfiguration.SSIStatisticsZipFileFolder;
            if (!_fileSystem.DirectoryExists(directoryPath)) 
            {
                status = HealthStatus.Unhealthy;
                data.Add("Directory for SSI statistics does not exist", directoryPath);

                return await Task.FromResult(new HealthCheckResult(
                    status,
                    Description,
                    data: data));
            }

            // Check latest file is from today
            var directory = new DirectoryInfo(directoryPath);
            var latestFileInfo = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (latestFileInfo == null)
            {
                status = HealthStatus.Unhealthy;
                data.Add($"SSI statistics file for today does not exist. Empty folder.", directoryPath);

                return await Task.FromResult(new HealthCheckResult(
                    status,
                    Description,
                    data: data));
            }
            var today = DateTime.Today.ToString("yyyy_MM_dd");
            if (!latestFileInfo.Name.Contains(today))
            {
                status = HealthStatus.Unhealthy;
                data.Add($"SSI statistics file for today does not exist. Latest file is {latestFileInfo.Name}. Today = {today}", directoryPath);
            }
            
            // Check numbers have been stored in database
            try
            {
                // check infection numbers
                var newestEntry = await _ssiIStatisticsRepository.GetNewestEntryAsync();
                if (newestEntry == null)
                {
                    status = HealthStatus.Unhealthy;
                    data.Add("SSI statistics infection entry in database does not exists", "CovidStatistics");

                    return await Task.FromResult(new HealthCheckResult(
                        status,
                        Description,
                        data: data));
                }
                var entryDate = newestEntry.Date;
                var entryDateString = entryDate.ToString("yyyy_MM_dd");
                if (!entryDateString.Contains(today))
                {
                    status = HealthStatus.Unhealthy;
                    data.Add($"SSI statistics infection entry in database is not from today {DateTime.Now}", $"Latest entry is from {entryDate}");
                }

                // check vaccine numbers
                var newestVaccineEntry = await _ssiISsiStatisticsVaccinationRepository.GetNewestEntryAsync();
                var vaccineEntryDate = newestVaccineEntry.Date;
                var vaccineEntryDateString = vaccineEntryDate.ToString("yyyy_MM_dd");
                if (!vaccineEntryDateString.Contains(today))
                {
                    status = HealthStatus.Unhealthy;
                    data.Add($"SSI statistics vaccine entry in database is not from today {DateTime.Now}", $"Latest entry is from {entryDate}");
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"{e.Message} - {e.StackTrace}";
                _logger.LogError(errorMessage);
                
                status = HealthStatus.Unhealthy;
                data.Add($"Error in data retrieval {DateTime.Now}", errorMessage);

                return await Task.FromResult(new HealthCheckResult(
                    status,
                    Description,
                    e,
                    data));
            }

            return await Task.FromResult(new HealthCheckResult(
                status,
                Description,
                data: data));
        }
    }
}
