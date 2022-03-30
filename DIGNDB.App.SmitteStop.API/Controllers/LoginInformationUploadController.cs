using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.API.Exceptions;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using DIGNDB.APP.SmitteStop.API.Exceptions;
using DIGNDB.APP.SmitteStop.Jobs.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API.Controllers
{
    /// <summary>
    /// Controller endpoint for uploading login information
    /// </summary>
    [ApiController]
    [Route("uploadLoginInformation")]

    public class LoginInformationUploadController : ControllerBase
    {
        private readonly ILogger<LoginInformationUploadController> _logger;
        private readonly ILoginInformationRepository _loginInformationRepository;

        /// <summary>
        /// Ctor for login information upload
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="loginInformationRepository"></param>
        public LoginInformationUploadController(ILogger<LoginInformationUploadController> logger, ILoginInformationRepository loginInformationRepository)
        {
            _logger = logger;
            _loginInformationRepository = loginInformationRepository;
        }

        /// <summary>
        /// Endpoint for 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TriforkControllerServerErrorException"></exception>
        /// <exception cref="TriforkControllerBadRequestException"></exception>
        [HttpPost]
        [ServiceFilter(typeof(TriforkAuthorizationAttribute))]
        public async Task<IActionResult> UploadLoginInformation()
        {
            _logger.LogInformation("Login information upload called");
            //var requestBody = string.Empty;
            //bool isAdded;
            //string logEntry;
            //try
            //{
            //    var loginInformation = await ReadRequestBody();
                
            //    var entry = CreateEntry(loginInformation);
            //    isAdded =_loginInformationRepository.CreateEntry(entry);
            //    logEntry = entry.ToString();
            //}
            //catch (HttpResponseException ex)
            //{
            //    _logger.LogError("Error when uploading login information: " + ex);
            //    throw;
            //}
            //catch (JsonException ex)
            //{
            //    _logger.LogError($"No login information found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
            //    return BadRequest("No login information found in body or unable to parse data");
            //}
            //catch (Exception ex)
            //{
            //    var errorMessage = $"An error occurred while trying to save login information: {ex}";
            //    _logger.LogError(errorMessage);
            //    return StatusCode(500);
            //}

            //var message = isAdded ? "Login information uploaded successfully. " + logEntry
            //                            : "Login information updated successfully. " + logEntry;
            //_logger.LogDebug(message);

            //return Ok(message);
            return Ok();
        }

        private static LoginInformation CreateEntry(LoginInformationDto loginInformation)
        {
            var attributes = loginInformation.events[0].attributes;

            var entry = new LoginInformation
            {
                Blocked = int.Parse(attributes.blokeret),
                Error = int.Parse(attributes.fejl),
                Negative = int.Parse(attributes.negativ),
                Positive = int.Parse(attributes.positiv),
                Timestamp = DateTime.UtcNow,
            };

            return entry;
        }

        private async Task<LoginInformationDto> ReadRequestBody()
        {
            string requestBody;
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var loginInformationObject = JsonSerializer.Deserialize<List<LoginInformationDto>>(
                requestBody,
                new JsonSerializerOptions {IgnoreNullValues = false});

            if (loginInformationObject == null || loginInformationObject.Count == 0)
            {
                throw new NullReferenceException("Body value for login information could not be serialized");
            }

            var loginInformation = loginInformationObject[0];
            if (loginInformation.events.Count > 1)
            {
                throw new TriforkControllerBadRequestException("Body value for login information has more than one event");
            }

            return loginInformation;
        }
    }
}
