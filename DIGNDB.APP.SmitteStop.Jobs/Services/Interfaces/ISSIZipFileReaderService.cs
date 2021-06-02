using DIGNDB.APP.SmitteStop.Jobs.Dto;

namespace DIGNDB.App.SmitteStop.Jobs.Services.Interfaces
{
    public interface ISSIZipFileReaderService
    {
        void HandleSsiVaccinationZipArchive(SSIZipArchivesInfoDto zipArchivesInfo);
        void HandleSsiStatisticsZipArchive(SSIZipArchivesInfoDto zipArchivesInfo);
    }
}
