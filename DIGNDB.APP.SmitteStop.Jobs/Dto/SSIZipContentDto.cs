using System.IO.Compression;

namespace DIGNDB.APP.SmitteStop.Jobs.Dto
{
    public class SSIZipContentDto
    {
        public ZipArchiveEntry DeathsOverTime { get; set; }
        public ZipArchiveEntry NewlyAdmittedOverTime { get; set; }
        public ZipArchiveEntry TestPosOverTime { get; set; }
        public ZipArchiveEntry Tested { get; set; }
        public ZipArchiveEntry Vaccinated { get; set; }
        public ZipArchiveEntry Statistics { get; set; }
    }
}