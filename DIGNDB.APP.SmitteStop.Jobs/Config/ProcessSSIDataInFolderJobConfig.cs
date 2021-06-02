using System.ComponentModel.DataAnnotations;

namespace DIGNDB.APP.SmitteStop.Jobs.Config
{
    public class ProcessSSIDataInFolderJobConfig : PeriodicJobBaseConfig
    {
        [Required]
        public SSIZipFolderProcessingConfig ZipFolderProcessingConfig { get; set; }

        [Required]
        public SSIExcelParsingConfig ExcelParsingConfig { get; set; }
    }
}
