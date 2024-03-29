﻿using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.API.Exceptions;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("updateApplicationStatistics")]
    public class ApplicationStatisticsController : ControllerBase
    {
        private readonly IApplicationStatisticsRepository _applicationStatisticsRepository;
        private readonly ILogger<ApplicationStatisticsController> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationStatisticsRepository"></param>
        /// <param name="logger"></param>
        public ApplicationStatisticsController(IApplicationStatisticsRepository applicationStatisticsRepository,
            ILogger<ApplicationStatisticsController> logger)
        {
            _applicationStatisticsRepository = applicationStatisticsRepository;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        [HttpPost]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        public async Task<IActionResult> UpdateApplicationStatistics()
        {
            _logger.LogInformation("Application statistics upload called");

            var requestBody = string.Empty;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                var applicationStatisticsDeserialized = JsonSerializer.Deserialize<ApplicationStatistics>(
                    requestBody,
                    new JsonSerializerOptions { IgnoreNullValues = false });

                if (applicationStatisticsDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }

                var newest = await _applicationStatisticsRepository.GetNewestEntryAsync();
                applicationStatisticsDeserialized.Id = newest.Id;
                applicationStatisticsDeserialized.EntryDate = DateTime.UtcNow;
                _applicationStatisticsRepository.UpdateEntry(applicationStatisticsDeserialized);
            }
            catch (HttpResponseException ex)
            {
                _logger.LogError("Error when uploading application statistics: " + ex);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No application statistics found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
                return BadRequest("No application statistics found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying to save application statistics: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode(500);
            }

            var message = "Application statistics uploaded successfully";
            _logger.LogDebug(message);
            return Ok(message);
        }
    }
}
