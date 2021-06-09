using DIGNDB.App.SmitteStop.Core.Configs;
using FederationGatewayApi.Config;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DIGNDB.APP.SmitteStop.Jobs.Config
{
    public class HangfireConfig
    {
        [Required(AllowEmptyStrings = false)]
        public string HangFireConnectionString { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string SmittestopConnectionString { get; set; }

        [Required]
        public List<string> ZipFilesFolders { get; set; }

        [Required]
        public int DaysToInvalidateZipFile { get; set; }

        [Required]
        public JobsConfig Jobs { get; set; }

        [Required]
        public EuGatewayConfig EuGateway { get; set; }

        [Required]
        public LoggingConfig Logging { get; set; }
        [Required]
        public TemporaryExposureKeyZipFilesSettings TemporaryExposureKeyZipFilesSettings { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LogsPath { get; set; }

        [Required(AllowEmptyStrings = false)]
        public int JobsMaxRetryAttempts { get; set; }

        [Required(AllowEmptyStrings = false)]
        public int JobsRetryInterval { get; set; }

    }

    public class JobsConfig
    {
        [Required]
        public PeriodicJobBaseConfig UpdateZipFiles { get; set; }

        [Required]
        public UploadKeysToGatewayJobConfig UploadKeysToTheGateway { get; set; }

        [Required]
        public DownloadKeysFromGatewayJobConfig DownloadKeysFromTheGateway { get; set; }

        [Required]
        public PeriodicJobBaseConfig RemoveOldZipFiles { get; set; }

        [Required]
        public ValidateKeysOnDatabaseConfig ValidateKeysOnDatabase { get; set; }

        [Required]
        public DailyMaintenanceCheckJobConfig DailyMaintenanceCheck { get; set; }

        [Required]
        public ProcessSSIDataInFolderJobConfig ProcessSSIDataInFolder { get; set; }
    }
}