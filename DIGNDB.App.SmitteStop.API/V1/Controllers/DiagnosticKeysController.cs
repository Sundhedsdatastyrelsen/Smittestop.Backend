﻿using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Dto;
using DIGNDB.App.SmitteStop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DIGNDB.App.SmitteStop.API
{
    [ApiController]
    [ApiVersion(ApiVersion)]
    [Route("v{version:apiVersion}/diagnostickeys")]
    [Route("diagnostickeys")]
    public class DiagnosticKeysController : ControllerBase
    {
        private const string ApiVersion = "1";

        private readonly IAppleService _appleService;
        private readonly IAddTemporaryExposureKeyService _addTemporaryExposureKeyService;
        private readonly ICacheOperations _cacheOperations;
        private readonly ITemporaryExposureKeyRepository _temporaryExposureKeyRepository;
        private readonly IExposureKeyValidator _exposureKeyValidator;
        private readonly ILogger _logger;
        private readonly IExposureConfigurationService _exposureConfigurationService;
        private readonly IKeyValidationConfigurationService _keyValidationConfigurationService;
        private readonly bool _enableCacheOverride;
        private readonly ICountryRepository _countryRepository;
        private readonly IAppSettingsConfig _appSettingsConfig;

        public DiagnosticKeysController(ICacheOperations cacheOperations, ILogger<DiagnosticKeysController> logger, IAppleService appleService,
            ITemporaryExposureKeyRepository temporaryExposureKeyRepository, IConfiguration configuration, IExposureKeyValidator exposureKeyValidator,
            IExposureConfigurationService exposureConfigurationService, IKeyValidationConfigurationService keyValidationConfigurationService,
            ICountryRepository countryRepository, IAppSettingsConfig appSettingsConfig, IAddTemporaryExposureKeyService addTemporaryExposureKeyService)
        {
            _addTemporaryExposureKeyService = addTemporaryExposureKeyService;
            _cacheOperations = cacheOperations;
            _temporaryExposureKeyRepository = temporaryExposureKeyRepository;
            _exposureKeyValidator = exposureKeyValidator;
            _logger = logger;
            _appleService = appleService;
            _exposureConfigurationService = exposureConfigurationService;
            _keyValidationConfigurationService = keyValidationConfigurationService;
            _appSettingsConfig = appSettingsConfig;
            _countryRepository = countryRepository;
            bool.TryParse(configuration["AppSettings:enableCacheOverride"], out _enableCacheOverride);
        }

        [HttpGet]
        [Route("exposureconfiguration")]
        [MapToApiVersion(ApiVersion)]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        public async Task<IActionResult> GetExposureConfiguration(bool r1_2, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("GetExposureConfiguration endpoint called");
                if (r1_2 == false)
                {
                    var exposureConfiguration = await _exposureConfigurationService.GetConfiguration();
                    _logger.LogInformation("V1.2 ExposureConfiguration fetched successfully");
                    return Ok(exposureConfiguration);
                }
                else
                {
                    var exposureConfiguration = await _exposureConfigurationService.GetConfigurationR1_2();
                    _logger.LogInformation("ExposureConfiguration fetched successfully");
                    return Ok(exposureConfiguration);
                }
            }
            catch (ArgumentException e)
            {
                _logger.LogError("Error: " + e);
                return BadRequest("Invalid exposure configuration or uninitialized");
            }
            catch (Exception e)
            {
                _logger.LogError("Error returning config:" + e);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Endpoint for uploading TEKs from the mobile app
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizationAttribute))]
        [MapToApiVersion(ApiVersion)]
        public async Task<IActionResult> UploadDiagnosisKeys()
        {
            var requestBody = string.Empty;

            try
            {
                _logger.LogInformation("UploadDiagnosisKeys endpoint called");
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                var parameter = JsonSerializer.Deserialize<TemporaryExposureKeyBatchDto>(requestBody);
                if (!parameter.keys.Any())
                {
                    _logger.LogWarning("Zero keys passed to upload endpoint");
                }

                _exposureKeyValidator.ValidateParameterAndThrowIfIncorrect(parameter, _keyValidationConfigurationService.GetConfiguration(), _logger);
                if (_appSettingsConfig.Configuration.GetValue<bool>("deviceVerificationEnabled"))
                {
                    await _exposureKeyValidator.ValidateDeviceVerificationPayload(parameter, _appleService, _logger);
                }

                var newTemporaryExposureKeys = await _addTemporaryExposureKeyService.GetFilteredKeysEntitiesFromDTO(parameter);
                if (newTemporaryExposureKeys.Any())
                {
                    var origin = _countryRepository.FindByIsoCode(parameter.regions[0]);
                    foreach (var key in newTemporaryExposureKeys)
                    {
                        key.Origin = origin;
                        key.KeySource = KeySource.SmitteStopApiVersion1;
                        key.ReportType = ReportType.CONFIRMED_TEST;
                    }

                    foreach (var newTemporaryExposureKey in newTemporaryExposureKeys)
                    {
                        var hexValue = BitConverter.ToString(newTemporaryExposureKey.KeyData);
                        _logger.LogInformation($"newTemporaryExposureKeys : {hexValue} , {newTemporaryExposureKey.DaysSinceOnsetOfSymptoms} , {newTemporaryExposureKey.Id}");
                    }
                    await _temporaryExposureKeyRepository.AddTemporaryExposureKeys(newTemporaryExposureKeys);
                }

                _logger.LogInformation("Keys uploaded successfully");
                return Ok();
            }
            catch (JsonException je)
            {
                _logger.LogError($"Incorrect JSON format: { je}  [Deserialized request]: {requestBody}");
                return BadRequest($"Incorrect JSON format: {je.Message}");
            }
            catch (ArgumentException ae)
            {
                _logger.LogError("Incorrect input format: " + ae);
                return BadRequest("Incorrect input format: " + ae.Message);
            }
            catch (SqlException e)
            {
                _logger.LogError("Error occured when uploading keys to the database." + e);
                return StatusCode(500, "Error occured when uploading keys to the database.");
            }
            catch (Exception e)
            {
                _logger.LogError("Error uploading keys:" + e);
                return StatusCode(500);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="packageName">A timestamp in the format of "yyyy-mm-dd"</param>
        /// <returns></returns>
        [HttpGet]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        [MapToApiVersion(ApiVersion)]
        [Route("{packageName}.zip")]
        public async Task<IActionResult> DownloadDiagnosisKeysFile(string packageName)
        {
            _logger.LogInformation("DownloadDiagnosisKeysFile endpoint called");
            try
            {
                int packageNumber = 0;
                DateTime packageDate;
                if (packageName.Equals("today", StringComparison.OrdinalIgnoreCase))
                {
                    packageDate = DateTime.UtcNow.Date;
                }
                else
                {
                    var packageDateStr = packageName.Split(":")[0];
                    if (packageName.Split(":").Length > 1)
                    {
                        Int32.TryParse(packageName.Split(":")[1], out packageNumber);
                    }

                    packageDate = DateTime.SpecifyKind(DateTime.Parse(packageDateStr).Date, DateTimeKind.Utc);
                }

                _logger.LogInformation("Package Date: " + packageDate);
                _logger.LogInformation("Add days: " + DateTime.UtcNow.Date.AddDays(-14));
                _logger.LogInformation("Utc now:" + DateTime.UtcNow);
                //verify date
                if (packageDate < DateTime.UtcNow.Date.AddDays(-14) || packageDate > DateTime.UtcNow)
                {
                    _logger.LogError($"Package Date is invalid date: {packageDate} packageName: {packageName}");
                    return BadRequest("Package Date is invalid");
                }
                CacheResult cacheResult = null;
                if (_enableCacheOverride && Request.Headers.ContainsKey("Cache-Control") && Request.Headers["Cache-Control"] == "no-cache")
                {
                    cacheResult = await _cacheOperations.GetCacheValue(packageDate, true);
                }
                else
                {
                    cacheResult = await _cacheOperations.GetCacheValue(packageDate);
                }

                //client should ask for the same file until batchcount increses. Then it should ask for that file with the higher batchcount.
                //Call for the day should stop, once a package with FinalForTheDay has been returned. then app can ask for next days package.
                Response.Headers.Add("Batchcount", cacheResult?.FileBytesList?.Count.ToString() ?? "0");
                var isLastPackage = (packageNumber == cacheResult?.FileBytesList?.Count - 1) ? true : false;
                bool finalForTheDay = packageDate.Date < DateTime.UtcNow.Date && isLastPackage && cacheResult != null;
                Response.Headers.Add("FinalForTheDay", finalForTheDay.ToString());

                //case where no keys for the day exists (yet)
                if (cacheResult != null && cacheResult.FileBytesList != null && !cacheResult.FileBytesList.Any())
                {
                    return NoContent();
                }

                if (cacheResult?.FileBytesList == null || packageNumber > cacheResult.FileBytesList.Count - 1)
                {
                    return NotFound();
                }

                _logger.LogInformation("Zip package fetched succesfully");
                //gets actual batch from todays batch-list
                return File(cacheResult.FileBytesList[packageNumber], "application/zip");
            }
            catch (SynchronizationLockException e)
            {
                _logger.LogError("Timout getting lock: " + e);
                return Accepted();
            }
            catch (FormatException e)
            {
                _logger.LogError("Error when parsing data: " + e);
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Error when downloading package: " + e);
                return StatusCode(500);
            }
        }
    }
}
