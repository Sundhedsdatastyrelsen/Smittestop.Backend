using System;

namespace DIGNDB.App.SmitteStop.Domain.Dto
{
    public class SSIStatisticsDto
    {
        public DateTime EntryDate { get; set; }
        public int ConfirmedCasesToday { get; set; }
        public int ConfirmedCasesTotal { get; set; }
        public int DeathsToday { get; set; }
        public int DeathsTotal { get; set; }
        public int TestsConductedToday { get; set; }
        public int TestsConductedTotal { get; set; }
        public int PatientsAdmittedToday { get; set; }
    }
}
