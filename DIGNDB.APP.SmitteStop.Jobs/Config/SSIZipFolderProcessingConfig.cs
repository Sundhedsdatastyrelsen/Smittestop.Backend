using System.ComponentModel.DataAnnotations;

namespace DIGNDB.APP.SmitteStop.Jobs.Config
{
    public class SSIZipFolderProcessingConfig
    {
        [Required]
        public string StatisticsZipFolderPath { get; set; }
        [Required]
        public string InfectionNumbersPrefix { get; set; }
        [Required]
        public string VaccinationNumbersPrefix { get; set; }
        [Required]
        public string StatisticsPrefix { get; set; }
        [Required]
        public string ZipPackageDatePattern { get; set; }
        [Required]
        public string ZipPackageDateParsingFormat { get; set; }
        [Required]
        public string ZipPackageVaccineDatePattern { get; set; }
        [Required]
        public string ZipPackageVaccineDateParsingFormat { get; set; }
    }
}
