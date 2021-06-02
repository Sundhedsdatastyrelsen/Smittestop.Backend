using System;

namespace DIGNDB.App.SmitteStop.Domain.Dto
{
    public class AppStatisticsDto
    {
        public DateTime EntryDate { get; set; }
        public int NumberOfPositiveTestsResultsLast7Days { get; set; }
        public int NumberOfPositiveTestsResultsTotal { get; set; }
        public int SmittestopDownloadsTotal { get; set; }
    }
}
