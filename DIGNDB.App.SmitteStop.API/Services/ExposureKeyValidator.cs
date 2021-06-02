using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Configuration;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ExposureKeyValidator : IExposureKeyValidator
    {
        private readonly ICountryRepository _countryRepository;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryRepository"></param>
        public ExposureKeyValidator(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ValidateParameterAndThrowIfIncorrect(TemporaryExposureKeyBatchDto parameter, KeyValidationConfiguration configuration, ILogger logger)
        {
            var todaysDateUtcMidnight = DateTime.UtcNow.Date;
            var outdatedKeysDate = DateTime.UtcNow.Date.AddDays(-configuration.OutdatedKeysDayOffset);
            //The period of time covered by the data file exceeds 14 days
            if (parameter.keys.Count > configuration.OutdatedKeysDayOffset)
            {
                var errorMessage = $"Incorrect key count. {parameter.keys.Count} > {configuration.OutdatedKeysDayOffset}";
                logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            if (!ValidateForDuplicatedKeys(parameter.keys.Select(k => k.key).ToList()))
            {
                throw new ArgumentException("Duplicate key values.");
            }

            if (parameter.keys.Any(x => x.key.Length != TemporaryExposureKeyDto.KeyLength))
            {
                parameter.keys = GetKeysWithValidSize(parameter.keys, logger);
            }

            foreach (var key in parameter.keys.Where(key => key.rollingStart < outdatedKeysDate || key.rollingStart > todaysDateUtcMidnight))
            {
                var errorMessage = $"Incorrect start date: {key.rollingStart} < {outdatedKeysDate} or {key.rollingStart} > {todaysDateUtcMidnight}";
                logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            //The TEKRollingPeriod value is not 144
            var tenMinutes  = TimeSpan.FromMinutes(10);
            foreach (var key in parameter.keys.Where(key => key.rollingDurationSpan > TemporaryExposureKeyDto.OneDayTimeSpan || key.rollingDurationSpan < tenMinutes))
            {
                var errorMessage = $"Incorrect span. {key.rollingDurationSpan} > {TemporaryExposureKeyDto.OneDayTimeSpan} or {key.rollingDurationSpan} < {tenMinutes}";
                logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            //Any ENIntervalNumber values from the same user are not unique
            var parametersGroups = parameter.keys.GroupBy(k => k.rollingStart);
            foreach (var group in parametersGroups)
            {
                if (group.Count() <= 1)
                {
                    continue;
                }

                var errorMessage = $"Incorrect intervals: {group.Count()} > 1";
                logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            //There are any gaps in the ENIntervalNumber values for a user
            //Any keys in the file have overlapping time windows
            if (parameter.keys.Any())
            {
                if (parameter.regions == null || !configuration.Regions.Contains(parameter.regions.Single()?.ToLower()))
                {
                    throw new ArgumentException("Incorrect region.");
                }
            }

            if (parameter.visitedCountries != null)
            {
                foreach (var visitedCountry in parameter.visitedCountries)
                {
                    var countryDb = _countryRepository.FindByIsoCode(visitedCountry);
                    if (countryDb == null || countryDb.VisitedCountriesEnabled == false)
                    {
                        throw new ArgumentException("Incorrect visited countries.");
                    }
                }
            }

            if (configuration.PackageNames.Apple != parameter.appPackageName.ToLower() && configuration.PackageNames.Google != parameter.appPackageName.ToLower())
            {
                throw new ArgumentException("Incorrect package name.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="appleService"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task ValidateDeviceVerificationPayload(TemporaryExposureKeyBatchDto parameter, IAppleService appleService, ILogger logger)
        {
            if (parameter.platform.ToLowerInvariant() == "ios")
            {
                AppleResponseDto apple = await appleService.ExecuteQueryBitsRequest(parameter.deviceVerificationPayload);
                if (apple.ResponseCode == HttpStatusCode.OK && !IsValidJson(apple.Content))
                {
                    apple = await appleService.ExecuteUpdateBitsRequest(parameter.deviceVerificationPayload);
                    if (apple.ResponseCode != HttpStatusCode.OK)
                    {
                        throw new ArgumentException("DeviceVerificationPayload invalid");
                    }
                    apple = await appleService.ExecuteQueryBitsRequest(parameter.deviceVerificationPayload);
                    if (apple.ResponseCode != HttpStatusCode.OK)
                    {
                        throw new ArgumentException("DeviceVerificationPayload invalid");
                    }
                }
                else if (apple.ResponseCode != HttpStatusCode.OK)
                {
                    throw new ArgumentException("DeviceVerificationPayload invalid");
                }
            }
            else if (parameter.platform.ToLowerInvariant() == "android")
            {
                try
                {
                    var attestationStatement = GoogleTokenValidator.ParseAndVerify(parameter.deviceVerificationPayload);
                    if (attestationStatement == null)
                    {
                        throw new Exception("Attestation statement empty.");
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException("DeviceVerificationPayload invalid. " + e);
                }
            }
            else
            {
                throw new ArgumentException($"DeviceVerificationPayload invalid platform. {parameter.platform}");
            }
        }

        #region private methods

        private static List<TemporaryExposureKeyDto> GetKeysWithValidSize(List<TemporaryExposureKeyDto> keys, ILogger logger)
        {
            var newNonValidExposureKeys = new List<TemporaryExposureKeyDto>();
            var newValidExposureKeys = new List<TemporaryExposureKeyDto>();

            foreach (var exposureKeyDto in keys)
            {
                if (exposureKeyDto.key.Length == TemporaryExposureKeyDto.KeyLength)
                {
                    newValidExposureKeys.Add(exposureKeyDto);
                }
                else
                {
                    newNonValidExposureKeys.Add(exposureKeyDto);
                }
            }

            ErrorLogKeysWithNonValidSizeInJsonFormat(newNonValidExposureKeys, logger);
            
            return newValidExposureKeys;
        }

        private static void ErrorLogKeysWithNonValidSizeInJsonFormat(List<TemporaryExposureKeyDto> newNonValidExposureKeys, ILogger logger)
        {
            var builder = new StringBuilder();
            builder.Append("[");
            builder.Append(newNonValidExposureKeys.First().ToJson());
            newNonValidExposureKeys.ForEach(x => builder.Append("," + x.ToJson()));
            builder.Append("]");
            logger.LogError($"Detected {newNonValidExposureKeys.Count} keys with an invalid size: {builder}");
        }

        private static bool ValidateForDuplicatedKeys(IList<byte[]> keys)
        {
            for (var i = 0; i < keys.Count - 1; i++)
            {
                for (var j = i + 1; j < keys.Count; j++)
                {
                    if (StructuralComparisons.StructuralEqualityComparer.Equals(keys[i], keys[j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            var inBrackets = strInput.StartsWith("{") && strInput.EndsWith("}");
            var inSquareBrackets = strInput.StartsWith("[") && strInput.EndsWith("]");
            if (!inBrackets && !inSquareBrackets)
            {
                return false;
            }

            try
            {
                JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}

