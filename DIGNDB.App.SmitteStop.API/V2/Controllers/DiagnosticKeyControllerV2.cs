using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Domain;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static System.String;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DIGNDB.App.SmitteStop.API
{
    /// <summary>
    /// TEK class
    /// </summary>
    [ApiController]
    [ApiVersion(_apiVersion)]
    [Route("v{version:apiVersion}/diagnostickeys")]
    public class DiagnosticKeysControllerV2 : ControllerBase
    {
        private readonly IAppleService _appleService;
        private readonly IAddTemporaryExposureKeyService _addTemporaryExposureKeyService;
        private readonly IConfiguration _configuration;
        private readonly IExposureKeyValidator _exposureKeyValidator;
        private readonly ILogger _logger;
        private readonly IExposureConfigurationService _exposureConfigurationService;
        private readonly IKeyValidationConfigurationService _keyValidationConfigurationService;
        private readonly IZipFileInfoService _zipFileInfoService;
        private readonly IAppSettingsConfig _appSettingsConfig;
        private readonly ICacheOperationsV2 _cacheOperations;

        private const string _apiVersion = "2";

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="appleService"></param>
        /// <param name="configuration"></param>
        /// <param name="exposureKeyValidator"></param>
        /// <param name="exposureConfigurationService"></param>
        /// <param name="keyValidationConfigurationService"></param>
        /// <param name="addTemporaryExposureKeyService"></param>
        /// <param name="zipFileInfoService"></param>
        /// <param name="appSettingsConfig"></param>
        /// <param name="cacheOperations"></param>
        public DiagnosticKeysControllerV2(ILogger<DiagnosticKeysControllerV2> logger, IAppleService appleService,
            IConfiguration configuration, IExposureKeyValidator exposureKeyValidator,
            IExposureConfigurationService exposureConfigurationService, IKeyValidationConfigurationService keyValidationConfigurationService,
            IAddTemporaryExposureKeyService addTemporaryExposureKeyService, IZipFileInfoService zipFileInfoService, IAppSettingsConfig appSettingsConfig,
            ICacheOperationsV2 cacheOperations)
        {
            _cacheOperations = cacheOperations;
            _configuration = configuration;
            _exposureKeyValidator = exposureKeyValidator;
            _logger = logger;
            _zipFileInfoService = zipFileInfoService;
            _appSettingsConfig = appSettingsConfig;
            _appleService = appleService;
            _exposureConfigurationService = exposureConfigurationService;
            _keyValidationConfigurationService = keyValidationConfigurationService;
            _addTemporaryExposureKeyService = addTemporaryExposureKeyService;
        }

        #region Smitte|stop API

        /// <summary>
        /// GAEN exposure configuration 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("exposureconfiguration")]
        [MapToApiVersion(_apiVersion)]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        public async Task<IActionResult> GetExposureConfiguration()
        {
            try
            {
                _logger.LogInformation("GetExposureConfiguration endpoint called");
                var exposureConfiguration = await _exposureConfigurationService.GetConfigurationR1_2();
                _logger.LogInformation("ExposureConfiguration fetched successfully");
                return Ok(exposureConfiguration);
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
        /// GAEN 2 exposure configuration
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("dailysummaryconfiguration")]
        [MapToApiVersion(_apiVersion)]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        public async Task<ActionResult> GetDailySummaryConfiguration()
        {
            try
            {
                _logger.LogInformation("GetDailySummaryConfiguration endpoint called");
                var exposureConfiguration = await _exposureConfigurationService.GetDailySummaryConfiguration();
                _logger.LogInformation("DailySummaryConfiguration fetched successfully");
                
                return Ok(exposureConfiguration);
            }
            catch (ArgumentException e)
            {
                _logger.LogError("Error: " + e);
                return BadRequest("Invalid DailySummaryConfiguration or uninitialized");
            }
            catch (Exception e)
            {
                _logger.LogError("Error returning DailySummaryConfiguration:" + e);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Upload TEK from mobile application
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizationAttribute))]
        [MapToApiVersion(_apiVersion)]
        public async Task<IActionResult> UploadDiagnosisKeys()
        {
            var requestBody = Empty;

            try
            {
                _logger.LogInformation("UploadDiagnosisKeys endpoint called");
                var parameters = await GetRequestParameters();
                await _addTemporaryExposureKeyService.CreateKeysInDatabase(parameters);

                _logger.LogInformation("Keys uploaded successfully");
                return Ok();
            }
            catch (JsonException je)
            {
                _logger.LogError($"Incorrect JSON format: {je} [Deserialized request]: {requestBody}");
                return BadRequest($"Incorrect JSON format: {je.Message}");
            }
            catch (ArgumentException ae)
            {
                _logger.LogError("Incorrect input format: " + ae);
                return BadRequest("Incorrect input format: " + ae.Message);
            }
            catch (SqlException e)
            {
                _logger.LogError("Error occurred when uploading keys to the database." + e);
                return StatusCode(500, "Error occurred when uploading keys to the database.");
            }
            catch (Exception e)
            {
                _logger.LogError("Error uploading keys:" + e);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Download zip file with TEKs
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        [HttpGet]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        [MapToApiVersion(_apiVersion)]
        [Route("{packageName}")]
        public async Task<IActionResult> DownloadDiagnosisKeysFile(string packageName)
        {
            _logger.LogInformation("DownloadDiagnosisKeysFile endpoint called");
            try
            {
                ZipFileInfo packageInfo = _zipFileInfoService.CreateZipFileInfoFromPackageName(packageName);
                string zipFilesFolder = _configuration["ZipFilesFolder"];

                _logger.LogInformation("Package Date: " + packageInfo.PackageDate);

                if (!IsDateValid(packageInfo.PackageDate, packageName))
                {
                    return BadRequest("Package Date is invalid");
                }
                _logger.LogInformation($"Zip files folder: {zipFilesFolder}");
                var packageExists = _zipFileInfoService.CheckIfPackageExists(packageInfo, zipFilesFolder);
                if (packageExists)
                {
                    byte[] zipFileContent;
                    var invalidateCache = false;
                    if (Request.Headers.ContainsKey("Cache-Control") && Request.Headers["Cache-Control"] == "no-cache")
                    {
                        invalidateCache = true;
                        zipFileContent = await _cacheOperations.GetCacheValue(packageInfo, zipFilesFolder, invalidateCache);
                    }
                    else
                    {
                        zipFileContent = await _cacheOperations.GetCacheValue(packageInfo, zipFilesFolder, invalidateCache);
                    }

                    var currentBatchNumber = packageInfo.BatchNumber;
                    packageInfo.BatchNumber++;
                    var nextPackageExists = _zipFileInfoService.CheckIfPackageExists(packageInfo, zipFilesFolder);

                    AddResponseHeader(nextPackageExists, currentBatchNumber);
                    _logger.LogInformation("Zip package fetched successfully");
                    return File(zipFileContent, System.Net.Mime.MediaTypeNames.Application.Zip);
                }
                else
                {
                    _logger.LogInformation("Package does not exist");
                    return NoContent();
                }
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

        #endregion

        #region Private methods

        private void AddResponseHeader(bool nextPackageExists, int lastBatchNumber)
        {
            Response.Headers.Add("nextBatchExists", nextPackageExists.ToString());
            Response.Headers.Add("lastBatchReturned", lastBatchNumber.ToString());
        }

        private bool IsDateValid(DateTime packageDate, string packageName)
        {
            if (packageDate < DateTime.UtcNow.Date.AddDays(-14) || packageDate > DateTime.UtcNow)
            {
                _logger.LogError($"Package Date is invalid date: {packageDate} packageName: {packageName}");
                return false;
            }
            return true;
        }

        private async Task<string> ReadRequestBody()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            return await reader.ReadToEndAsync();
        }

        private async Task<TemporaryExposureKeyBatchDto> GetRequestParameters()
        {
            var requestBody = await ReadRequestBody();
            var parameters = JsonSerializer.Deserialize<TemporaryExposureKeyBatchDto>(requestBody);
            
            LogParameters(parameters);

            var keyValidationConfiguration = _keyValidationConfigurationService.GetConfiguration();
            _exposureKeyValidator.ValidateParameterAndThrowIfIncorrect(parameters, keyValidationConfiguration, _logger);

            if (_appSettingsConfig.Configuration.GetValue<bool>("deviceVerificationEnabled"))
            {
                await _exposureKeyValidator.ValidateDeviceVerificationPayload(parameters, _appleService, _logger);
            }

            if (parameters.keys.Count == 0)
            {
                _logger.LogWarning("Zero keys passed to upload endpoint");
            }

            return parameters;
        }

        private void LogParameters(TemporaryExposureKeyBatchDto parameters)
        {
            try
            {
                var jsonValue = parameters.ToJson();
                _logger.LogInformation($"Uploaded keys: {jsonValue}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not serialize uploaded keys: {e.Message} - {e.StackTrace}");
            }
        }
    }

    #endregion
}
