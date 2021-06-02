using Microsoft.Extensions.Configuration;

namespace DIGNDB.App.SmitteStop.Core
{
    public interface IAppSettingsConfig
    {
        IConfiguration Configuration { get; }
    }
}