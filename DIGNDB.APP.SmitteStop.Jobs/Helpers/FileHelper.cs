using System.Collections.Generic;
using System.Linq;

namespace DIGNDB.APP.SmitteStop.Jobs.Helpers
{
    public class FileHelper
    {
        public static bool DoesFileNamesContainOnlyZips(List<string> fileNames)
        {
            return fileNames.All(fileName => fileName.EndsWith(".zip"));
        }
    }
}
