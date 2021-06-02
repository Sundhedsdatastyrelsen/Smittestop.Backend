using Microsoft.Extensions.Configuration;

namespace DIGNDB.App.SmitteStop.Core
{
    public class AppSettingsConfig : IAppSettingsConfig
    {
        private readonly IConfiguration _configuration;
        public const string AppSettingsSectionName = "AppSettings";

        public AppSettingsConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration => _configuration.GetSection(AppSettingsSectionName);
    }
}
