

using System.ComponentModel.DataAnnotations;

namespace DIGNDB.APP.SmitteStop.API.Config
{
    /// <summary>
    /// Class for UploadFileValidationRules values related to header values
    /// </summary>
    public class UploadFileValidationRules
    {
        /// <summary>
        /// UploadFileValidationRules value used when check file size
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string FileSize { get; set; }

        /// <summary>
        /// UploadFileValidationRules value used when check file name
        /// </summary>
        [Required(AllowEmptyStrings = false)] 
        public string InfectionFileRegexPattern { get; set; }

        /// <summary>
        /// UploadFileValidationRules value used when check file name
        /// </summary>
        [Required(AllowEmptyStrings = false)] 
        public string VaccinationFileRegexPattern { get; set; }

        /// <summary>
        /// UploadFileValidationRules value for path validation
        /// </summary>
        [Required(AllowEmptyStrings = false)] 
        public string PathNamesIgnoreByVerification { get; set; }
    }
}