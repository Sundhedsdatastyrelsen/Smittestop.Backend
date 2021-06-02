using CsvHelper.Configuration;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;
using DIGNDB.APP.SmitteStop.Jobs.Services.TypeConverters;

namespace DIGNDB.APP.SmitteStop.Jobs.ClassMaps
{
    public sealed class NewlyAdmittedOverTimeDtoMap : ClassMap<NewlyAdmittedOverTimeDto>
    {
        public NewlyAdmittedOverTimeDtoMap(SSIExcelParsingConfig config)
        {
            Map(m => m.DateString).Name(config.DateColumnNames);
            Map(m => m.Hospitalized).Name(config.NewlyAdmittedOverTime.HospitalizedColumnNames).TypeConverter<IntConverter>();
        }
    }
}