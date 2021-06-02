using DIGNDB.App.SmitteStop.Domain.Configuration;
using DIGNDB.APP.SmitteStop.API.Config;
using Microsoft.Extensions.Logging;

namespace DIGNDB.App.SmitteStop.API.Attributes
{
    public class TriforkAuthorizationAttribute : ApiKeyAuthorizationAttribute
    {
        public TriforkAuthorizationAttribute(AuthOptions authOptions, ILogger<TriforkAuthorizationAttribute> logger, ApiConfig config) : base(authOptions, logger)
        {
            this._tokenJsonKey = "Authorization_Trifork";
            this._tokenEncrypted = config.AppSettings.AuthorizationTrifork;
        }
    }
}
