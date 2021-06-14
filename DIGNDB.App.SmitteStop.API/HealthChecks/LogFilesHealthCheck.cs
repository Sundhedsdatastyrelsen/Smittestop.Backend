using DIGNDB.App.SmitteStop.API.HealthCheckAuthorization;
using DIGNDB.APP.SmitteStop.API.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API.HealthChecks
{
    /// <summary>
    /// Implementation of IHealthCheck for log files health checks
    /// </summary>
    [Authorize(Policy = HealthCheckBasicAuthenticationHandler.HealthCheckBasicAuthenticationScheme)]
    public class LogFilesHealthCheck : IHealthCheck
    {
        private const string Description = "Health check for log files";

        private readonly ApiConfig _apiConfiguration;
        private readonly ILogger<LogFilesHealthCheck> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string _logFilesDatePattern;
        private readonly Regex _apiRegex;
        private readonly Regex _jobsRegex;
        private readonly string _mobileLogFilesDatePattern;
        private readonly Regex _mobileRegex;

        /// <summary>
        /// Ctor 
        /// </summary>
        /// <param name="apiConfiguration"></param>
        /// <param name="logger"></param>
        /// <param name="httpContextAccessor"></param>
        public LogFilesHealthCheck(ApiConfig apiConfiguration, ILogger<LogFilesHealthCheck> logger, IHttpContextAccessor httpContextAccessor)
        {
            _apiConfiguration = apiConfiguration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            // Initialize Health Check AppSettings
            var apiRegex = apiConfiguration.HealthCheckSettings.ApiRegex;
            _apiRegex = new Regex(apiRegex);
            var jobsRegex = apiConfiguration.HealthCheckSettings.JobsRegex;
            _jobsRegex = new Regex(jobsRegex);
            var mobileRegex = apiConfiguration.HealthCheckSettings.MobileRegex;
            _mobileRegex = new Regex(mobileRegex);
            _logFilesDatePattern = apiConfiguration.HealthCheckSettings.LogFilesDatePattern;
            _mobileLogFilesDatePattern = apiConfiguration.HealthCheckSettings.MobileLogFilesDatePattern;
        }

        /// <summary>
        /// Checks access to log files and that they are written to every day
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation("Health check HangFire");

            var status = HealthStatus.Healthy;
            var data = new Dictionary<string, object>();

            try
            {
                // Api log files
                var apiLogFilesNamePrefix = _apiConfiguration.AppSettings.LogsApiPath;
                CheckLogFiles(apiLogFilesNamePrefix, _apiRegex, ref status, data, _logFilesDatePattern);

                // Jobs log files
                var query = _httpContextAccessor.HttpContext.Request.Query;
                if (QueryContainsWfe01AndMachineIsWfe01(query))
                {
                    var jobsLogFilesNamePrefix = _apiConfiguration.AppSettings.LogsJobsPath;
                    CheckLogFiles(jobsLogFilesNamePrefix, _jobsRegex, ref status, data, _logFilesDatePattern);
                }

                // Mobile log files
                var mobileLogFilesNamePrefix = _apiConfiguration.AppSettings.LogsMobilePath; // see also log4net.config where this is configured
                CheckLogFiles(mobileLogFilesNamePrefix, _mobileRegex, ref status, data, _mobileLogFilesDatePattern);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} - {e.StackTrace}");
                return Task.FromResult(new HealthCheckResult(
                    status,
                    Description,
                    e,
                    data));
            }

            return Task.FromResult(new HealthCheckResult(
                status,
                Description,
                data: data));
        }

        private void CheckLogFiles(string logFilesNamePrefix, Regex logFilesRegex, ref HealthStatus status,
            IDictionary<string, object> data, string datePattern)
        {
            var logFilesDirectoryName = Path.GetDirectoryName(logFilesNamePrefix);
            if (!Directory.Exists(logFilesDirectoryName))
            {
                status = HealthStatus.Unhealthy;
                data.Add($"Could not find log files for {logFilesNamePrefix}", $"Folder for {logFilesNamePrefix} does not exist");
            }
            else if (logFilesDirectoryName == null)
            {
                status = HealthStatus.Unhealthy;
                data.Add($"Argument {nameof(logFilesDirectoryName)} is null or is a root directory {DateTime.Now}", "");
            }
            else
            {
                var logFilesDirectory = new DirectoryInfo(logFilesDirectoryName);
                var logFiles = logFilesDirectory.GetFiles().OrderByDescending(f => f.LastWriteTime);
                var orderedLogFiles = logFiles.Where(n => logFilesRegex.IsMatch(n.Name));
                var orderedLogFilesList = orderedLogFiles.ToList();
                if (!orderedLogFilesList.Any()) // No log files
                {
                    status = HealthStatus.Unhealthy;
                    data.Add($"No log files for {logFilesNamePrefix}", "");
                }
                else // No log files today
                {
                    var today = DateTime.Today.ToString(datePattern);
                    var first = orderedLogFilesList.First();
                    var fileForToday = first.Name.Contains(today);
                    if (fileForToday)
                    {
                        return;
                    }

                    status = HealthStatus.Unhealthy;
                    data.Add($"No log file for today {logFilesNamePrefix}", "");
                }
            }
        }

        private bool QueryContainsWfe01AndMachineIsWfe01(IQueryCollection query)
        {
            query.TryGetValue("server", out var server);
            var queryContainsWfe01 = server == "wfe01";

            var name = Environment.MachineName;
            var isWfe01 = name.ToLower().Contains("wfe01");

#if DEBUG
            isWfe01 = true;
#endif

            _logger.LogInformation($"|Health check log files| Server name: {name}; Field for server name 'isWfe01' value: {isWfe01}; Query contains 'wfe01': {queryContainsWfe01}; Query: {query}");

            return queryContainsWfe01 && isWfe01;
        }
    }
}
