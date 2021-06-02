using CsvHelper.Configuration;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;
using DIGNDB.APP.SmitteStop.Jobs.Services.TypeConverters;

namespace DIGNDB.APP.SmitteStop.Jobs.ClassMaps
{
    public sealed class DeathsOverTimeDtoMap : ClassMap<DeathsOverTimeDto>
    {
        public DeathsOverTimeDtoMap(SSIExcelParsingConfig config)
        {
            Map(m => m.DateString).Name(config.DateColumnNames);
            Map(m => m.Deaths).Name(config.DeathsOverTime.DeathsColumnNames).TypeConverter<IntConverter>();
        }
    }
}