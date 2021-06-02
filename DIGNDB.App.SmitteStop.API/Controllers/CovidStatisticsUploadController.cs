using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.APP.SmitteStop.API.Config;
using DIGNDB.APP.SmitteStop.API.Exceptions;
using DIGNDB.APP.SmitteStop.Jobs.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API.Controllers
{
    /// <summary>
    /// Endpoint for uploading Covid statistics file from SSI
    /// </summary>
    [ApiController]
    [Route("uploadStatistics")]

    public class CovidStatisticsUploadController : ControllerBase
    {
        private readonly ILogger<CovidStatisticsUploadController> _logger;
        private readonly ApiConfig _config;
        private readonly IFileSystem _fileSystem;
        private readonly IUploadFileValidationService _fileValidator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="fileSystem"></param>
        /// <param name="fileValidator"></param>
        public CovidStatisticsUploadController(ILogger<CovidStatisticsUploadController> logger, ApiConfig config, IFileSystem fileSystem, IUploadFileValidationService fileValidator)
        {
            _fileSystem = fileSystem;
            _config = config;
            _logger = logger;
            _fileValidator = fileValidator;
        }

        /// <summary>
        /// POST endpoint for uploading Covid statistics from SSI
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TriforkControllerServerErrorException"></exception>
        /// <exception cref="TriforkControllerBadRequestException"></exception>
        [HttpPost]
        [ServiceFilter(typeof(TriforkAuthorizationAttribute))]
        public async Task<IActionResult> UploadCovidStatistics()
        {
            _logger.LogInformation("File upload called");
            try
            {
                if (!_fileSystem.DirectoryExists(_config.SSIStatisticsZipFileFolder))
                {
                    throw new TriforkControllerServerErrorException(
                        "Server error: The file save directory is not reachable or does not exist");
                }

                var sentFiles = Request.Form.Files;
                if (sentFiles.Count != 1)
                {
                    throw new TriforkControllerBadRequestException("Files count is not equal to 1");
                }

                var file = sentFiles.First();

                if(!_fileValidator.Verify(file, out var message)) // validation of file 
                {
                    throw new TriforkControllerBadRequestException(message);
                }

                var destinationPath = Path.Combine(_config.SSIStatisticsZipFileFolder, file.FileName);
                try
                {
                    await _fileSystem.SaveFormFileAsync(file, destinationPath);
                    _logger.LogInformation($"File uploaded completed successfully: {file.FileName}");
                    return Ok();
                }
                catch (Exception e)
                {
                    var errorMessage = $"Server error: Error when trying to save zip file: {e.Message} - {e.StackTrace}";
                    _logger.LogError(errorMessage);
                    throw new TriforkControllerServerErrorException("Server error: Error when trying to save zip file", e);
                }
            }
            catch (TriforkControllerBadRequestException e)
            {
                _logger.LogError($"Bad request with covid statistics was sent from Trifork. Error: {e}");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError($"Internal server error when trying to process request with covid statistics from Trifork. Error: {e}");
                return StatusCode(500, e.Message);
            }
        }
    }
}
