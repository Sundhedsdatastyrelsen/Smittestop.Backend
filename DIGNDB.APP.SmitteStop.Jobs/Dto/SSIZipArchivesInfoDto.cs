using System;
using System.IO.Compression;

namespace DIGNDB.APP.SmitteStop.Jobs.Dto
{
    public class SSIZipArchivesInfoDto : IDisposable
    {
        public DateTime DateInfection { get; set; }

        public DateTime DateVaccination { get; set; }

        public ZipArchive TodayArchive { get; set; }
        public ZipArchive YesterdayArchive { get; set; }
        public ZipArchive VaccinationArchive { get; set; }
        public ZipArchive StatisticsArchive { get; set; }

        public void Dispose()
        {
            TodayArchive?.Dispose();
            YesterdayArchive?.Dispose();
        }
    }

}