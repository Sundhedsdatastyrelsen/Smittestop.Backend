using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DIGNDB.App.SmitteStop.API.Services
{
    /// <summary>
    /// Validates uploaded mobile log message
    /// </summary>
    public class LogMessageValidator : ILogMessageValidator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logMessageMobile"></param>
        /// <exception cref="JsonException"></exception>
        public void ValidateLogMobileMessageReportedTime(LogMessageMobile logMessageMobile)
        {
            var reportedTime = logMessageMobile.reportedTime;
            if (!DateTime.TryParse(reportedTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                throw new JsonException($"Could not parse reportedTime: {reportedTime}");
            }
            
            if (!IsValidDateTimeFormat(result))
            {
                throw new ArgumentException($"Value of reportedTime is more than two years from: {reportedTime}", nameof(logMessageMobile.reportedTime));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logMessageMobile"></param>
        /// <param name="patternsDictionary"></param>
        /// <exception cref="JsonException"></exception>
        public void ValidateLogMobileMessagePatterns(LogMessageMobile logMessageMobile, IDictionary<string, string> patternsDictionary)
        {
            var elements = logMessageMobile.GetPatternsValueToVerify(patternsDictionary);
            foreach (var (key, value) in elements)
            {
                if (!IsValidRegularExpression(value))
                {
                    throw new JsonException($"Invalid pattern: {key}:{value}");
                }
            }
        }

        private bool IsValidRegularExpression(KeyValuePair<string, string> keyValuePair)
        {
            Regex r = new Regex($"{keyValuePair.Key}");
            return r.IsMatch(keyValuePair.Value);
        }

        private static bool IsValidDateTimeFormat(DateTime reportedTime)
        {
            var timeSpan = (DateTime.UtcNow - reportedTime).Duration();
            var years = timeSpan.TotalDays / 365.25;
            return !(years > 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lm"></param>
        /// <param name="maxLength"></param>
        public void SanitizeAndShortenTextFields(LogMessageMobile lm, int maxLength)
        {
            SanitizeAndShortenField(lm.api, s => lm.api = s, maxLength);
            SanitizeAndShortenField(lm.exceptionType, s => lm.exceptionType = s, maxLength);
            SanitizeAndShortenField(lm.exceptionMessage, s => lm.exceptionMessage = s, maxLength);
            SanitizeAndShortenField(lm.exceptionStackTrace, s => lm.exceptionStackTrace = s, maxLength);
            SanitizeAndShortenField(lm.innerExceptionType, s => lm.innerExceptionType = s, maxLength);
            SanitizeAndShortenField(lm.innerExceptionMessage, s => lm.innerExceptionMessage = s, maxLength);
            SanitizeAndShortenField(lm.innerExceptionStackTrace, s => lm.innerExceptionStackTrace = s, maxLength);
            SanitizeAndShortenField(lm.severity, s => lm.severity = s, maxLength);
            SanitizeAndShortenField(lm.description, s => lm.description = s, maxLength);
            SanitizeAndShortenField(lm.buildVersion, s => lm.buildVersion = s, maxLength);
            SanitizeAndShortenField(lm.buildNumber, s => lm.buildNumber = s, maxLength);
            SanitizeAndShortenField(lm.deviceType, s => lm.deviceType = s, maxLength);
            SanitizeAndShortenField(lm.deviceDescription, s => lm.deviceDescription = s, maxLength);
            SanitizeAndShortenField(lm.deviceOSVersion, s => lm.deviceOSVersion = s, maxLength);
            SanitizeAndShortenField(lm.apiErrorMessage, s => lm.apiErrorMessage = s, maxLength);
            SanitizeAndShortenField(lm.additionalInfo, s => lm.additionalInfo = s, maxLength);
            SanitizeAndShortenField(lm.correlationId, s => lm.correlationId= s, maxLength);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setFieldOnLogMessage"></param>
        /// <param name="maxLength"></param>
        public void SanitizeAndShortenField(string value, Action<string> setFieldOnLogMessage, int maxLength)
        {
            if (String.IsNullOrWhiteSpace(value)) return;
            var modifiedField = Sanitizer.GetSafeHtmlFragment(value.Trim());
            bool valueHasChanged = (!modifiedField.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (modifiedField.Length > maxLength)
            {
                modifiedField = modifiedField.Substring(0,maxLength);
                valueHasChanged = true;
            }
            if (valueHasChanged)
            {
                setFieldOnLogMessage(modifiedField);
            }
        }
    }
}
