using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.API.Exceptions;
using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DIGNDB.App.SmitteStop.API
{
    /// <summary>
    /// Logging controller endpoint for uploading mobile logs
    /// </summary>
    [ApiController]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/logging")]
    public class LoggingControllerV2 : ControllerBase
    {
        private readonly ILogger<LoggingControllerV2> _logger;
        private readonly ILogMessageValidator _logMessageValidator;
        private readonly IDictionary<string, string> _logMobilePatternsDictionary;
        private readonly log4net.ILog _loggerMobile;
        private readonly int _maxTextFieldLength;
        private readonly bool _logEndpointOverride;
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logMessageValidator"></param>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public LoggingControllerV2(ILogMessageValidator logMessageValidator, ILogger<LoggingControllerV2> logger, IConfiguration configuration)
        {
            _logMessageValidator = logMessageValidator;
            _logger = logger;
            _logMobilePatternsDictionary = InitializePatternDictionary(configuration);
            _loggerMobile = MobileLoggerFactory.GetLogger();
            int.TryParse(configuration["LogValidationRules:maxTextFieldLength"], out _maxTextFieldLength);
            bool.TryParse(configuration["AppSettings:logEndpointOverride"], out _logEndpointOverride);
        }
        
        /// <summary>
        /// Endpoint for uploading mobile device logs
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        [Route("logMessages")]
        public async Task<IActionResult> UploadMobileLogs()
        {
            if (_logEndpointOverride)
            {
                return Ok();
            }

            _logger.LogDebug("LogMessage action invoked");
            
            LogMessagesMobileCollection logMessagesMobileDeserialized;
            var requestBody = string.Empty;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                logMessagesMobileDeserialized = JsonSerializer.Deserialize<LogMessagesMobileCollection>(requestBody, new JsonSerializerOptions { IgnoreNullValues = false });
            }
            catch (JsonException je)
            {
                _logger.LogError($"Deserializing Request.Body caused an error. [RequestBody]: {requestBody} [Message]: {je.Message} [Exception]: {je.StackTrace}");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"Handling Request.Body cause an error. [RequestBody]: {requestBody} [Message]: {e.Message} [Exception]: {e.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var responseCode = SanitizeAndValidateLogMessages(logMessagesMobileDeserialized);

            return StatusCode(responseCode);
        }

        /// <summary>
        /// Sanitizes and validates log messages from the mobile device.
        /// In case of multiple exceptions the response code corresponding to the last exception thrown will be returned.
        /// However, all logs will be validated and possible exceptions logged.
        /// </summary>
        /// <param name="logMessagesMobileDeserialized"></param>
        /// <returns></returns>
        private int SanitizeAndValidateLogMessages(LogMessagesMobileCollection logMessagesMobileDeserialized)
        {
            var responseCode = StatusCodes.Status200OK;

            foreach (var logMessageMobile in logMessagesMobileDeserialized.logs)
            {
                try
                {
                    _logMessageValidator.SanitizeAndShortenTextFields(logMessageMobile, _maxTextFieldLength == 0 ? 500 : _maxTextFieldLength);
                    _logMessageValidator.ValidateLogMobileMessagePatterns(logMessageMobile, _logMobilePatternsDictionary);
                    _logMessageValidator.ValidateLogMobileMessageReportedTime(logMessageMobile);
                    switch (logMessageMobile.severity)
                    {
                        case "INFO":
                            _loggerMobile.Info(logMessageMobile);
                            break;
                        case "ERROR":
                            _loggerMobile.Error(logMessageMobile);
                            break;
                        case "WARNING":
                            _loggerMobile.Warn(logMessageMobile);
                            break;
                    }
                }
                catch (HttpResponseException ex)
                {
                    _logger.LogError("Error when uploading mobile logs" + ex);
                    throw;
                }
                catch (ArgumentException ae)
                {
                    _logger.LogError($"[Message]: {ae.Message} [Exception]: {ae.StackTrace}");
                    responseCode = StatusCodes.Status400BadRequest;
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"[Message]: {ex.Message} [Exception]: {ex.StackTrace} [Deserialized request]: {logMessageMobile}");
                    responseCode = StatusCodes.Status400BadRequest;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while trying to save the logs. [Message]: {ex.Message} [Exception]: {ex.StackTrace} [Deserialized request]: {logMessageMobile}");
                    responseCode = StatusCodes.Status500InternalServerError;
                }
            }

            return responseCode;
        }

        private IDictionary<string, string> InitializePatternDictionary(IConfiguration configuration)
        {
            var logMobilePatternsDictionary = new Dictionary<string, string>();
            logMobilePatternsDictionary.Add("severityRegex", configuration["LogValidationRules:severityRegex"]);
            logMobilePatternsDictionary.Add("positiveNumbersRegex", configuration["LogValidationRules:positiveNumbersRegex"]);
            logMobilePatternsDictionary.Add("buildVersionRegex", configuration["LogValidationRules:buildVersionRegex"]);
            logMobilePatternsDictionary.Add("operationSystemRegex", configuration["LogValidationRules:operationSystemRegex"]);
            logMobilePatternsDictionary.Add("deviceOSVersionRegex", configuration["LogValidationRules:deviceOSVersionRegex"]);
            return logMobilePatternsDictionary;
        }
    }
}
