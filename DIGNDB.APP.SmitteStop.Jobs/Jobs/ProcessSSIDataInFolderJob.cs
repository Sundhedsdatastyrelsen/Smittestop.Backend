using DIGNDB.App.SmitteStop.Jobs.Services.Interfaces;
using StackExchange.Profiling;

namespace DIGNDB.APP.SmitteStop.Jobs.Jobs
{
    public class ProcessSsiDataInFolderJob
    {
        private readonly ISSIZipFileReaderService _ssiZipFileReaderService;
        private readonly ISSIZipFolderHandlingService _ssiZipFolderHandlingService;

        public ProcessSsiDataInFolderJob(ISSIZipFileReaderService ssiZipFileReaderService, ISSIZipFolderHandlingService ssiZipFolderHandlingService)
        {
            _ssiZipFolderHandlingService = ssiZipFolderHandlingService;
            _ssiZipFileReaderService = ssiZipFileReaderService;
        }

        public void UpdateCovidStatistics()
        {
            using var zipArchives = _ssiZipFolderHandlingService.GetNewestArchivesFromFolder();

            //if (_ssiZipFolderHandlingService.CheckAreBothFilesPresents(zipArchives))
            //{
            //    return;   TODO  revert this comment when a client accept this change
            //}
            using (MiniProfiler.Current.Step("Job/SSI"))
            {
                using (MiniProfiler.Current.Step("Job/SSI/Vaccine"))
                {
                    _ssiZipFileReaderService.HandleSsiVaccinationZipArchive(zipArchives);
                }

                using (MiniProfiler.Current.Step("Job/SSI/Statistic"))
                {
                    _ssiZipFileReaderService.HandleSsiStatisticsZipArchive(zipArchives);
                }
            }
        }
    }
}