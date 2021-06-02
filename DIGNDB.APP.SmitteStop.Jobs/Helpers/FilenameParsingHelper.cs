using DIGNDB.APP.SmitteStop.Jobs.Exceptions;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DIGNDB.APP.SmitteStop.Jobs.Helpers
{
    public class FilenameParsingHelper
    {
        public static int FilenameWithExtractionDateComparer(string value1, string value2, string zipPackageDatePattern, string zipPackageDateParsingFormat)
        {
            DateTime date1;
            DateTime date2;
            try
            {
                date1 = ExtractDateFromFilename(value1, zipPackageDatePattern, zipPackageDateParsingFormat);
            }
            catch (SsiZipFolderProcessingException)
            {
                return -1;
            }

            try
            {
                date2 = ExtractDateFromFilename(value2, zipPackageDatePattern, zipPackageDateParsingFormat);
            }
            catch (SsiZipFolderProcessingException)
            {
                return 1;
            }

            return date1.CompareTo(date2);
        }

        public static DateTime ExtractDateFromFilename(string filename, string zipPackageDatePattern, string zipPackageDateParsingFormat)
        {
            var zipPackageDateMatches = Regex.Matches(filename, zipPackageDatePattern);
            if (zipPackageDateMatches.Count == 0 || zipPackageDateMatches.Count > 1)
            {
                throw new SsiZipFolderProcessingException($"Unable to extract date from zip file name");
            }
            return DateTime.ParseExact(zipPackageDateMatches[0].Value, zipPackageDateParsingFormat, CultureInfo.InvariantCulture);
        }
    }
}
