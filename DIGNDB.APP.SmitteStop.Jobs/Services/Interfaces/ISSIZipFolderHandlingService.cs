using DIGNDB.APP.SmitteStop.Jobs.Dto;

namespace DIGNDB.App.SmitteStop.Jobs.Services.Interfaces
{
    public interface ISSIZipFolderHandlingService
    {
        public SSIZipArchivesInfoDto GetNewestArchivesFromFolder();
    }
}
