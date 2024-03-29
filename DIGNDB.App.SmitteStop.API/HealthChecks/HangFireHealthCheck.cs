﻿using DIGNDB.App.SmitteStop.API.HealthCheckAuthorization;
using DIGNDB.App.SmitteStop.Core.Contracts;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DIGNDB.APP.SmitteStop.API.Config;

namespace DIGNDB.App.SmitteStop.API.HealthChecks
{
    /// <summary>
    /// Implementation of IHealthCheck for HangFire health checks
    /// </summary>
    [Authorize(Policy = HealthCheckBasicAuthenticationHandler.HealthCheckBasicAuthenticationScheme)]
    public class HangFireHealthCheck : IHealthCheck
    {
        private const string Description = "HangFire health check inspects No. of servers and failed jobs";

        private readonly ApiConfig _apiConfiguration;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HangFireHealthCheck> _logger;

        private readonly IMonitoringApi _hangFireMonitoringApi;
        private readonly IHealthCheckHangFireService _healthCheckHangFireService;
        private static JobStorage _jobStorageCurrent;

        /// <summary>
        /// Ctor initializing HangFire monitoring API object
        /// </summary>
        public HangFireHealthCheck(ApiConfig apiConfiguration, IConfiguration configuration, ILogger<HangFireHealthCheck> logger, IHealthCheckHangFireService healthCheckHangFireService)
        {
            _apiConfiguration = apiConfiguration;
            _configuration = configuration;
            _logger = logger;
            _hangFireMonitoringApi = healthCheckHangFireService.GetHangFireMonitoringApi();
            _healthCheckHangFireService = healthCheckHangFireService;

            if (_jobStorageCurrent == null)
            {
                InitializeHangFire();
            }

            if (_jobStorageCurrent != null)
            {
                _hangFireMonitoringApi = _jobStorageCurrent.GetMonitoringApi();
            }
        }

        /// <summary>
        /// Checks various HangFire settings, i.e., failing jobs and a positive number of servers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation("Health check HangFire");

            var status = HealthStatus.Healthy;
            var data = new Dictionary<string, object>();
            
            // Check servers
            var servers = _hangFireMonitoringApi.Servers();
            if (servers.Count < 1)
            {
                var message = $"Number of servers is less than expected at time {DateTime.Now}";
                _logger.LogWarning(message);
                status = HealthStatus.Unhealthy;
                data.Add(message, servers.Count);
            }

            // Check status of recurring jobs
            var recurringJobs = _healthCheckHangFireService.GetRecurringJobs();
            var noOfActuallyEnabledJobs = recurringJobs.Count(j => j.NextExecution != null);
            var noOfExpectedEnabledJobs = _apiConfiguration.HealthCheckSettings.NoOfEnabledJobs;
            if (noOfActuallyEnabledJobs < noOfExpectedEnabledJobs)
            {
                var message = $"Number of enabled jobs is less than expected at time {DateTime.Now}. Expected:{noOfExpectedEnabledJobs} - Actual:{noOfActuallyEnabledJobs}";
                _logger.LogWarning(message);
                status = HealthStatus.Unhealthy;
                data.Add(message, noOfActuallyEnabledJobs);

                var enabledJobIds = _apiConfiguration.HealthCheckSettings.EnabledJobIds.Where(j => j != "");
                foreach (var jobId in enabledJobIds)
                {
                    var job = recurringJobs.FirstOrDefault(j => j.Id == jobId);
                    if (job.NextExecution != null)
                    {
                        continue;
                    }

                    var jobMessage = $"Job '{jobId}' is not enabled as expected";
                    _logger.LogWarning(jobMessage);
                    status = HealthStatus.Unhealthy;
                    var info = job.LastExecution != null
                        ? $"Last execution time: {job.LastExecution}"
                        : $"No last execution time for {jobId}";
                    data.Add(jobMessage, info);
                }
            }

            // Check failing jobs
            var failingJobsCount = _hangFireMonitoringApi.FailedCount();
            if (failingJobsCount > 0)
            {
                status = HealthStatus.Unhealthy;
                data.Add("failed jobs count", failingJobsCount);

                _logger.LogWarning("HangFire has one or more failed jobs");
                
                var jobs = _hangFireMonitoringApi.FailedJobs(0, 10);
                foreach (var job in jobs)
                {
                    var key = job.Key;
                    var val = job.Value;
                    _logger.LogWarning($"Failed job ID '{key}' - {val.ExceptionDetails} - {val.ExceptionMessage}");

                    try
                    {
                        var serializedVal = JsonSerializer.Serialize(val, val.GetType());
                        data.Add($"Job ID {key}", serializedVal);
                    }
                    catch (Exception e)
                    {
                        var errorMessage = $"Error in health check HangFire. {e.Message} - {e.StackTrace}";
                        _logger.LogError(errorMessage);

                        // Adding extra details from the failed job to data
                        var failedJobDetails = $"JOB FAILED: EXCEPTION DETAILS: {val.ExceptionDetails} - EXCEPTION MESSAGE: {val.ExceptionMessage} - EXCEPTION TYPE: {val.ExceptionType} - FAILED AT: {val.FailedAt}";
                        data.Add($"Job ID {key}. HangFire health check could not serialize class {val.GetType()}", failedJobDetails);

                        return Task.FromResult(new HealthCheckResult(
                            status,
                            Description,
                            e,
                            data));
                    }
                }
            }

            if (data.Count == 0)
            {
                data.Add("State", "HangFire jobs and servers are in expected state");
            }

            return Task.FromResult(new HealthCheckResult(
                status,
                Description,
                data: data));
        }

        private void InitializeHangFire()
        {
            var connectionString = _configuration["HangFireConnectionString"];
            JobStorage.Current = new SqlServerStorage(connectionString);
            _jobStorageCurrent = JobStorage.Current;
        }
    }
}
