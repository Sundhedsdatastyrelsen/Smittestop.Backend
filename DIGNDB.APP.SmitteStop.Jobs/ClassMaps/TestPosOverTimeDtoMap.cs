using CsvHelper.Configuration;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;
using DIGNDB.APP.SmitteStop.Jobs.Services.TypeConverters;

namespace DIGNDB.APP.SmitteStop.Jobs.ClassMaps
{
    public sealed class TestPosOverTimeDtoMap : ClassMap<TestPosOverTimeDto>
    {
        public TestPosOverTimeDtoMap(SSIExcelParsingConfig config)
        {
            Map(m => m.DateString).Name(config.DateColumnNames);
            Map(m => m.ConfirmedCases).Name(config.TestPosOverTime.ConfirmedCasesColumnNames).TypeConverter<IntConverter>();
        }
    }
}