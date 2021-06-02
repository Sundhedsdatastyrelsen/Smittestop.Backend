using System;

namespace DIGNDB.App.SmitteStop.Domain.Db
{
    public partial class SSIStatistics
    {
        public int Id { get; set; }
        public int ConfirmedCasesToday { get; set; }
        public int ConfirmedCasesTotal { get; set; }
        public int DeathsToday { get; set; }
        public int DeathsTotal { get; set; }
        public int TestsConductedToday { get; set; }
        public int TestsConductedTotal { get; set; }
        public DateTime Date { get; set; }
    }
}
