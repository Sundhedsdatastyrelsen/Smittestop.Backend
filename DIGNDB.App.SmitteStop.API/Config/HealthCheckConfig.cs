using System.ComponentModel.DataAnnotations;

namespace DIGNDB.APP.SmitteStop.API.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class HealthCheckConfig
    {
        /// <summary>
        /// 
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public int NumbersTodayCallAfter24Hour { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public int ZipFilesCallAfter24Hour { get; set; }

        /// <summary>
        /// Appsettings value for api log file name
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string ApiRegex { get; set; }

        /// <summary>
        /// Appsettings value jobs log file name
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string JobsRegex { get; set; }

        /// <summary>
        /// Appsettings value mobile logs file name
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string MobileRegex { get; set; }

        /// <summary>
        /// Appsettings value log files date pattern
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string LogFilesDatePattern { get; set; }

        /// <summary>
        /// Appsettings value mobile log files date pattern
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string MobileLogFilesDatePattern { get; set; }
    }
}