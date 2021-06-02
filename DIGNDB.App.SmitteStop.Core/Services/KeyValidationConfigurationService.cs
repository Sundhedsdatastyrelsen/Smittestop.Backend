using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Domain;
using DIGNDB.App.SmitteStop.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Linq;

namespace DIGNDB.App.SmitteStop.Core.Services
{
    public class KeyValidationConfigurationService : IKeyValidationConfigurationService
    {
        private readonly KeyValidationConfiguration _keyValidationConfiguration;
        
        private const string OutdatedKeysDayOffsetConfigurationKey = "KeyValidationRules:OutdatedKeysDayOffset";
        private const string RegionsConfigurationKey = "Regions";
        private const string KeyValidationRulesConfigurationKey = "KeyValidationRules";
        private const string PackageNamesConfigurationKey = "PackageNames";
        private const string AndroidConfigurationKey = "android";
        private const string IosConfigurationKey = "ios";

        public KeyValidationConfigurationService()
        {
            _keyValidationConfiguration = new KeyValidationConfiguration();
        }

        public void SetConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var isParsingSuccessful = int.TryParse(configuration[OutdatedKeysDayOffsetConfigurationKey],
                out int parsedOutdatedKeysDayOffset);
            if (!isParsingSuccessful)
                throw new ConfigurationErrorsException(
                    $"Cannot parse {OutdatedKeysDayOffsetConfigurationKey} from configuration");
            _keyValidationConfiguration.OutdatedKeysDayOffset = parsedOutdatedKeysDayOffset;

            var parsedRegions = configuration.GetSection(KeyValidationRulesConfigurationKey)
                .GetSection(RegionsConfigurationKey)
                .GetChildren()
                .Select(x => x.Value)
                .ToList();
            if (!parsedRegions.Any())
                throw new ConfigurationErrorsException(
                    $"Cannot parse {KeyValidationRulesConfigurationKey}:{RegionsConfigurationKey} from configuration");
            _keyValidationConfiguration.Regions = parsedRegions;

            _keyValidationConfiguration.PackageNames = GetPackageNames(configuration);
        }

        private PackageNameConfig GetPackageNames(IConfiguration configuration)
        {
            var androidPackageName = configuration
                .GetSection(KeyValidationRulesConfigurationKey)
                .GetSection(PackageNamesConfigurationKey)
                .GetSection(AndroidConfigurationKey)
                .Value;
            if (string.IsNullOrWhiteSpace(androidPackageName))
                throw new ConfigurationErrorsException(
                    $"Cannot parse {KeyValidationRulesConfigurationKey}:{PackageNamesConfigurationKey}:{AndroidConfigurationKey} from configuration");

            var iosPackageName = configuration
                .GetSection(KeyValidationRulesConfigurationKey)
                .GetSection(PackageNamesConfigurationKey)
                .GetSection(IosConfigurationKey)
                .Value;
            if (string.IsNullOrWhiteSpace(iosPackageName))
                throw new ConfigurationErrorsException(
                    $"Cannot parse {KeyValidationRulesConfigurationKey}:{PackageNamesConfigurationKey}:{IosConfigurationKey} from configuration");

            var result = new PackageNameConfig
            {
                Google = androidPackageName,
                Apple = iosPackageName
            };
            ;
            return result;
        }

        public KeyValidationConfiguration GetConfiguration()
        {
            return _keyValidationConfiguration ?? throw new ArgumentException("Exposure Configuration is not initialized");
        }
    }
}
