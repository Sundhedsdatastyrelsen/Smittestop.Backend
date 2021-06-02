namespace DIGNDB.APP.SmitteStop.Jobs.Config
{
    public class SSIExcelParsingConfig
    {
        public NewlyAdmittedOverTimeConfig NewlyAdmittedOverTime { get; set; }
        public TestPosOverTimeConfig TestPosOverTime { get; set; }
        public TestedExcelFileConfig Tested { get; set; }
        public DeathsOverTimeConfig DeathsOverTime { get; set; }
        public VaccinatedExcelFileConfig Vaccinated { get; set; }
        public CovidStatistics CovidStatistics { get; set; }
        public string[] DateColumnNames { get; set; }
        public string[] TotalColumnNames { get; set; }
        public string Culture { get; set; }
    }
}
