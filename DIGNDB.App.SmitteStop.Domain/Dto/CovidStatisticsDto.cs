namespace DIGNDB.App.SmitteStop.Domain.Dto
{
    public class CovidStatisticsDto
    {
        public SSIStatisticsDto SSIStatistics { get; set; }
        public SSIStatisticsVaccinationDto SsiStatisticsVaccination { get; set; }
        public AppStatisticsDto AppStatistics { get; set; }
    }
}
