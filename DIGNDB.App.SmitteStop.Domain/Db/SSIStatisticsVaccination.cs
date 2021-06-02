using System;

namespace DIGNDB.App.SmitteStop.Domain.Db
{
    public class SSIStatisticsVaccination
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double VaccinationFirst { get; set; }
        public double VaccinationSecond { get; set; }
    }
}
