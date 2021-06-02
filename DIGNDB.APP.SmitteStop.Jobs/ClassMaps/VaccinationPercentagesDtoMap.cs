using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;

namespace DIGNDB.APP.SmitteStop.Jobs.ClassMaps
{
    public sealed class VaccinationPercentagesDtoMap : ClassMap<VaccinationPercentagesDto>
    {
        public VaccinationPercentagesDtoMap(SSIExcelParsingConfig config)
        {
            Map(m => m.FirstTime).Name(config.Vaccinated.VaccinatedFirstTimeColumnName).TypeConverter<DoubleConverter>(); 
            Map(m => m.SecondTime).Name(config.Vaccinated.VaccinatedSecondTimeColumnName).TypeConverter<DoubleConverter>();
        }
    }
}