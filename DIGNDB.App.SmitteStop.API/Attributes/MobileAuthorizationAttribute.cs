using DIGNDB.App.SmitteStop.Domain.Configuration;
using DIGNDB.APP.SmitteStop.API.Config;
using Microsoft.Extensions.Logging;

namespace DIGNDB.App.SmitteStop.API.Attributes
{
    /// <summary>
    /// Attribute providing access security to endpoints
    /// </summary>
    public class MobileAuthorizationAttribute : ApiKeyAuthorizationAttribute
    {
        /// <summary>
        /// Ctor for setting tokens JsonKey and Encrypted in base class
        /// </summary>
        /// <param name="authOptions"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public MobileAuthorizationAttribute(AuthOptions authOptions, ILogger<MobileAuthorizationAttribute> logger, ApiConfig config) : base(authOptions, logger)
        {
            _tokenJsonKey = "Authorization_Mobile";
            _tokenEncrypted = config.AppSettings.AuthorizationMobile;
        }
    }
}
