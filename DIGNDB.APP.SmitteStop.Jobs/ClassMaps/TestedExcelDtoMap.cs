using CsvHelper.Configuration;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;
using DIGNDB.APP.SmitteStop.Jobs.Services.TypeConverters;

namespace DIGNDB.APP.SmitteStop.Jobs.ClassMaps
{
    public sealed class TestedExcelDtoMap : ClassMap<TestedExcelDto>
    {
        public TestedExcelDtoMap(SSIExcelParsingConfig config)
        {
            Map(m => m.DateString).Name(config.DateColumnNames);
            Map(m => m.Tested).Name(config.Tested.TestedColumnNames).TypeConverter<IntConverter>();
        }
    }
}