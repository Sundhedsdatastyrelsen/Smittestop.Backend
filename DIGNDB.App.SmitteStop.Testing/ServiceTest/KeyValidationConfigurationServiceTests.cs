using DIGNDB.App.SmitteStop.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest
{
    [TestFixture]
    public class KeyValidationConfigurationServiceTests
    {
        private readonly KeyValidationConfigurationService _service = new KeyValidationConfigurationService();

        [Test]
        public void SetConfiguration_ShouldThrowConfigurationExceptionIfOutdatedKeysDayOffsetIsMissingFromConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            Action setConfigurationAction = () => _service.SetConfiguration(configuration);
            setConfigurationAction.Should().Throw<ConfigurationErrorsException>()
                .WithMessage("Cannot parse KeyValidationRules:OutdatedKeysDayOffset from configuration");
        }

        [Test]
        public void SetConfiguration_ShouldThrowConfigurationExceptionIfRegionsMissingFromConfiguration()
        {
            var inMemorySettings =
                new Dictionary<string, string> {
                    {"KeyValidationRules:OutdatedKeysDayOffset", "1"},
                };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action setConfigurationAction = () => _service.SetConfiguration(configuration);
            setConfigurationAction.Should().Throw<ConfigurationErrorsException>()
                .WithMessage("Cannot parse KeyValidationRules:Regions from configuration");
        }

        [Test]
        public void SetConfiguration_ShouldThrowConfigurationExceptionIfConfigurationIsNull()
        {
            Action setConfigurationAction = () => _service.SetConfiguration(null);
            setConfigurationAction.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'configuration')");
        }

        [Test]
        public void SetConfiguration_ShouldThrowConfigurationExceptionIfAndroidPackageNameMissingFromConfiguration()
        {
            var inMemorySettings =
                new Dictionary<string, string> {
                    {"KeyValidationRules:OutdatedKeysDayOffset", "1"},
                    {"KeyValidationRules:Regions:0", "dk"},
                    {"KeyValidationRules:Regions:1", "pl"},
                    {"KeyValidationRules:Regions:2", "fr"},
                    {"KeyValidationRules:PackageNames:ios", "smittestop.ios"},
                };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action setConfigurationAction = () => _service.SetConfiguration(configuration);
            setConfigurationAction.Should().Throw<ConfigurationErrorsException>()
                .WithMessage("Cannot parse KeyValidationRules:PackageNames:android from configuration");
        }

        [Test]
        public void SetConfiguration_ShouldThrowConfigurationExceptionIfIosPackageNameMissingFromConfiguration()
        {
            var inMemorySettings =
                new Dictionary<string, string> {
                    {"KeyValidationRules:OutdatedKeysDayOffset", "1"},
                    {"KeyValidationRules:Regions:0", "dk"},
                    {"KeyValidationRules:Regions:1", "pl"},
                    {"KeyValidationRules:Regions:2", "fr"},
                    {"KeyValidationRules:PackageNames:android", "smittestop.android"},
                };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            Action setConfigurationAction = () => _service.SetConfiguration(configuration);
            setConfigurationAction.Should().Throw<ConfigurationErrorsException>()
                .WithMessage("Cannot parse KeyValidationRules:PackageNames:ios from configuration");
        }
    }
}