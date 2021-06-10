using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;

namespace DIGNDB.APP.SmitteStop.Jobs.ClassMaps
{
    public sealed class StatisticsDtoMap : ClassMap<StatisticsDto>
    {
        public StatisticsDtoMap(SSIExcelParsingConfig config)
        {
            Map(m => m.ConfirmedCases).Name(config.CovidStatistics.ColumnNames[0]).TypeConverter<Int32Converter>();
            Map(m => m.Died).Name(config.CovidStatistics.ColumnNames[1]).TypeConverter<Int32Converter>();
            Map(m => m.ChangedConfirmedCases).Name(config.CovidStatistics.ColumnNames[2]).TypeConverter<Int32Converter>();
            Map(m => m.ChangedDied).Name(config.CovidStatistics.ColumnNames[3]).TypeConverter<Int32Converter>();
            Map(m => m.NumberSamples).Name(config.CovidStatistics.ColumnNames[4]).TypeConverter<Int32Converter>();
            Map(m => m.ChangedNumberSamplesPcr).Name(config.CovidStatistics.ColumnNames[5]).TypeConverter<Int32Converter>();
            Map(m => m.ChangedNumberSamplesAntigen).Name(config.CovidStatistics.ColumnNames[6]).TypeConverter<Int32Converter>();
        }
    }
}