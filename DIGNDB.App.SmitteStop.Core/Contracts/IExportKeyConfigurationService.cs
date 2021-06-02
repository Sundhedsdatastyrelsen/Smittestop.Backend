using DIGNDB.App.SmitteStop.Domain;
using Microsoft.Extensions.Configuration;

namespace DIGNDB.App.SmitteStop.Core.Contracts
{
    public interface IExportKeyConfigurationService
    {
        void SetConfiguration(IConfiguration configuration);
        ExportKeyConfiguration GetConfiguration();
    }
}
