using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace DIGNDB.APP.SmitteStop.Jobs.Services.TypeConverters
{
    public class IntConverter : DefaultTypeConverter, ITypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return -1;
            }

            if (int.TryParse(text.Replace(".", string.Empty), out int i))
            {
                return i;
            }

            return -1;
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value.ToString();
        }
    }
}
