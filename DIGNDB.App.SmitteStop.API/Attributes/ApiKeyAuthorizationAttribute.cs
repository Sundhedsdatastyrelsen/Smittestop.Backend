using DIGNDB.App.SmitteStop.API.HealthCheckAuthorization;
using DIGNDB.App.SmitteStop.Core.Helpers;
using DIGNDB.App.SmitteStop.Domain.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace DIGNDB.App.SmitteStop.API.Attributes
{
    /// <summary>
    /// Super class for adding specific attributes to authorized endpoints 
    /// </summary>
    public class ApiKeyAuthorizationAttribute : ActionFilterAttribute
    {
        private readonly AuthOptions _authOptions;
        private readonly ILogger<ApiKeyAuthorizationAttribute> _logger;
        
        /// <summary>
        /// Name of header value. Overwrite in implementing class
        /// </summary>
        protected string _tokenJsonKey;

        /// <summary>
        /// Value taken from configuration appsettings. Overwrite in implementing class
        /// </summary>
        protected string _tokenEncrypted;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="authOptions"></param>
        /// <param name="logger"></param>
        public ApiKeyAuthorizationAttribute(AuthOptions authOptions, ILogger<ApiKeyAuthorizationAttribute> logger)
        {
            _authOptions = authOptions;
            _logger = logger;
        }

        /// <summary>
        /// Handles authorization by inspecting token in header for value set in <see cref="_tokenJsonKey"/>, <seealso cref="HealthCheckAuthorizationHandler"/>
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Used in, e.g., development
            if (!_authOptions.MobileAuthHeaderCheckEnabled)
            {
                return;
            }

            if (context.HttpContext.Request.Headers.TryGetValue(_tokenJsonKey, out StringValues values))
            {
                if (!values.Any())
                {
                    context.Result = new UnauthorizedObjectResult($"Missing value for {_tokenJsonKey} token");
                }
                else
                {
                    var token = values.First();
                    string triforkTokenDecrypted;

                    try
                    {
                        triforkTokenDecrypted = ConfigEncryptionHelper.UnprotectString(_tokenEncrypted);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(
                            $"Configuration error. Cannot decrypt the {_tokenJsonKey}. Encrypted token: {_tokenEncrypted}. Message: {e.Message}");
                        throw;
                    }

                    if (!token.Equals(triforkTokenDecrypted) || string.IsNullOrEmpty(triforkTokenDecrypted))
                    {
                        context.Result = new UnauthorizedObjectResult($"invalid {_tokenJsonKey} token");
                    }
                }
            }
            else
            {
                context.Result = new UnauthorizedObjectResult($"Missing {_tokenJsonKey} token");
            }
        }
    }
}
