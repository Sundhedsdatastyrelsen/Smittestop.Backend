using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Core.Helpers;
using DIGNDB.App.SmitteStop.Core.Models;
using DIGNDB.App.SmitteStop.Domain.Dto;
using DIGNDB.App.SmitteStop.Domain.Dto.DailySummaryConfiguration;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Core.Services
{
    public class ExposureConfigurationService : IExposureConfigurationService
    {
        private ExposureConfiguration _exposureConfiguration;
        private ExposureConfigurationV1_2 _exposureConfigurationV1_2;
        private DailySummaryExposureConfiguration _dailySummaryConfiguration;

        public ExposureConfigurationService()
        {
            _exposureConfiguration = new ExposureConfiguration();
        }

        public ExposureConfiguration RetrieveExposureConfigurationFromConfig(IConfiguration configuration, string configSectionName)
        {
            ExposureConfiguration exposureConfiguration = new ExposureConfiguration();
            exposureConfiguration.AttenuationScores = configuration.GetSection(configSectionName)
                .GetSection("AttenuationScores").GetChildren().Select(x => int.Parse(x.Value)).ToArray();
            exposureConfiguration.AttenuationWeight = int.Parse(configuration[$"{configSectionName}:AttenuationWeight"]);
            exposureConfiguration.DaysSinceLastExposureScores = configuration.GetSection($"{configSectionName}")
                .GetSection("DaysSinceLastExposureScores").GetChildren().Select(x => int.Parse(x.Value)).ToArray();
            exposureConfiguration.DaysSinceLastExposureWeight =
                int.Parse(configuration[$"{configSectionName}:DaysSinceLastExposureWeight"]);
            exposureConfiguration.MinimumRiskScore = int.Parse(configuration[$"{configSectionName}:MinimumRiskScore"]);
            exposureConfiguration.DurationAtAttenuationThresholds = configuration.GetSection($"{configSectionName}")
                .GetSection("DurationAtAttenuationThresholds").GetChildren().Select(x => int.Parse(x.Value)).ToArray();
            exposureConfiguration.DurationScores = configuration.GetSection($"{configSectionName}")
                .GetSection("DurationScores").GetChildren().Select(x => int.Parse(x.Value)).ToArray();
            exposureConfiguration.DurationWeight = int.Parse(configuration[$"{configSectionName}:DurationWeight"]);
            exposureConfiguration.TransmissionRiskScores = configuration.GetSection($"{configSectionName}")
                .GetSection("TransmissionRiskScores").GetChildren().Select(x => int.Parse(x.Value)).ToArray();
            exposureConfiguration.TransmissionRiskWeight =
                int.Parse(configuration[$"{configSectionName}:TransmissionRiskWeight"]);
            return exposureConfiguration;
        }

        public void SetConfiguration(IConfiguration configuration)
        {
            // v1
            _exposureConfiguration = RetrieveExposureConfigurationFromConfig(configuration, "ExposureConfig");

            // v2
            _exposureConfigurationV1_2 = new ExposureConfigurationV1_2()
            {
                Configuration = RetrieveExposureConfigurationFromConfig(configuration, "ExposureConfigV1_2"),
                AttenuationBucketsParams = RetrieveAttentuationBucketsParametersFromConfig(configuration)
            };

            // DailySummaryConfiguration
            _dailySummaryConfiguration = RetrieveDailySummaryConfiguration(configuration);
            ModelValidator.ValidateContract(_dailySummaryConfiguration);
        }

        private DailySummaryExposureConfiguration RetrieveDailySummaryConfiguration(IConfiguration configuration)
        {
            var dailySummaryConfigurationSection = configuration.GetSection("DailySummaryConfiguration");
            var dailySummaryConfiguration = dailySummaryConfigurationSection.Get<DailySummaryConfiguration>();
            var scoreSumThreshold = configuration.GetValue<double>("ScoreSumThreshold");

            var retVal = new DailySummaryExposureConfiguration
            {
                DailySummaryConfiguration = dailySummaryConfiguration,
                ScoreSumThreshold = scoreSumThreshold
            };

            return retVal;
        }

        public async Task<ExposureConfiguration> GetConfiguration()
        {
            if (_exposureConfiguration == null)
            {
                throw new ArgumentException("Exposure Configuration is not initialized");
            }

            return await Task.FromResult(_exposureConfiguration);
        }

        public async Task<ExposureConfigurationV1_2> GetConfigurationR1_2()
        {
            return await Task.FromResult(_exposureConfigurationV1_2);
        }

        public async Task<DailySummaryExposureConfiguration> GetDailySummaryConfiguration()
        {
            return await Task.FromResult(_dailySummaryConfiguration);
        }

        private AttenuationBucketsParams RetrieveAttentuationBucketsParametersFromConfig(IConfiguration configuration)
        {
            var config = new AttenuationBucketsParams();
            var sectionName = "AttenuationBucketsParams";
            config.ExposureTimeThreshold = double.Parse(configuration[$"{sectionName}:ExposureTimeThreshold"], CultureInfo.InvariantCulture);
            config.HighAttenuationBucketMultiplier = double.Parse(configuration[$"{sectionName}:HighAttenuationBucketMultiplier"], CultureInfo.InvariantCulture);
            config.LowAttenuationBucketMultiplier = double.Parse(configuration[$"{sectionName}:LowAttenuationBucketMultiplier"], CultureInfo.InvariantCulture);
            config.MiddleAttenuationBucketMultiplier = double.Parse(configuration[$"{sectionName}:MiddleAttenuationBucketMultiplier"], CultureInfo.InvariantCulture);
            return config;
        }
    }
}
