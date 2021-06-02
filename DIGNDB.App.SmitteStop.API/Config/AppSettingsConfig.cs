using System.ComponentModel.DataAnnotations;

namespace DIGNDB.APP.SmitteStop.API.Config
{
    /// <summary>
    /// Class for appsettings values related to header values
    /// </summary>
    public class AppSettingsConfig
    {
        /// <summary>
        /// Appsettings value used when user has been authenticated
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string AuthorizationTrifork { get; set; }

        /// <summary>
        /// Appsettings value used when accessing api endpoints
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string AuthorizationMobile{ get; set; }

        /// <summary>
        /// Appsettings value used when accessing health check endpoints
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string AuthorizationHealthCheck { get; set; }

        /// <summary>
        /// Appsettings value for path to API log files
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string LogsApiPath { get; set; }

        /// <summary>
        /// Appsettings value for path to Jobs log files
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string LogsJobsPath { get; set; }

        /// <summary>
        /// Appsettings value for path to Mobile log files
        /// </summary>
        [Required]
        public string LogsMobilePath { get; set; }

    }
}