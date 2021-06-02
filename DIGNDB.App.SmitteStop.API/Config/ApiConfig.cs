using DIGNDB.App.SmitteStop.Core.Configs;
using System.ComponentModel.DataAnnotations;

namespace DIGNDB.APP.SmitteStop.API.Config
{
    /// <summary>
    /// Config settings for the API project
    /// </summary>
    public class ApiConfig
    {
        /// <summary>
        /// App settings values
        /// </summary>
        [Required]
        public AppSettingsConfig AppSettings { get; set; }

        /// <summary>
        /// Health check settings
        /// </summary>
        [Required]
        public HealthCheckConfig HealthCheckSettings { get; set; }

        /// <summary>
        /// Config settings for folder to store SSI files
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string SSIStatisticsZipFileFolder { get; set; }

        /// <summary>
        /// Config settings for TEK zip files 
        /// </summary>
        [Required]
        public TemporaryExposureKeyZipFilesSettings TemporaryExposureKeyZipFilesSettings { get; set; }

        /// <summary>
        /// Config setting for folder to store zip files
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string ZipFilesFolder { get; set; }

        /// <summary>
        /// Config settings for file validation
        /// </summary>
        [Required]
        public UploadFileValidationRules UploadFileValidationRules { get; set; }
    }
}