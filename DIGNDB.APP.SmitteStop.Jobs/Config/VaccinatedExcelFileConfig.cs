namespace DIGNDB.APP.SmitteStop.Jobs.Config
{
    public class VaccinatedExcelFileConfig : ExcelConfig
    {
        public string VaccinatedFirstTimeColumnName { get; set; }
        public string VaccinatedSecondTimeColumnName { get; set; }
        public string VaccinationCulture { get; set; }
    }
}
