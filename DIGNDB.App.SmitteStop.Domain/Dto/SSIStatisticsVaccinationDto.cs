using System;

namespace DIGNDB.App.SmitteStop.Domain.Dto
{
    public class SSIStatisticsVaccinationDto
    {
        public DateTime EntryDate { get; set; }
        public double VaccinationFirst { get; set; }
        public double VaccinationSecond { get; set; }
    }
}
