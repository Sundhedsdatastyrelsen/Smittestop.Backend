using AutoMapper;
using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DIGNDB.App.SmitteStop.API
{
    [ApiController]
    [ApiVersion(_apiVersion)]
    [Route("v{version:apiVersion}/covidstatistics")]
    public class CovidStatisticsController : ControllerBase
    {
        private readonly IApplicationStatisticsRepository _applicationStatisticsRepository;
        private readonly ILogger _logger;
        private readonly ISSIStatisticsRepository _ssiIStatisticsRepository;
        private readonly ISSIStatisticsVaccinationRepository _ssiStatisticsVaccinationRepository;
        private readonly IMapper _mapper;

        private const string _apiVersion = "2";

        public CovidStatisticsController(ILogger<CovidStatisticsController> logger, IApplicationStatisticsRepository applicationStatisticsRepository,
            ISSIStatisticsRepository ssiIStatisticsRepository, ISSIStatisticsVaccinationRepository ssiStatisticsVaccinationRepository, IMapper mapper)
        {
            _mapper = mapper;
            _ssiIStatisticsRepository = ssiIStatisticsRepository;
            _ssiStatisticsVaccinationRepository = ssiStatisticsVaccinationRepository;
            _applicationStatisticsRepository = applicationStatisticsRepository;
            _logger = logger;
        }

        [HttpGet]
        [MapToApiVersion(_apiVersion)]
        [ServiceFilter(typeof(MobileAuthorizationAttribute))]
        public async Task<IActionResult> GetCovidStatistics(string packageDate)
        {
            try
            {
                var applicationStatisticsDb = await _applicationStatisticsRepository.GetNewestEntryAsync();
                if (applicationStatisticsDb == null)
                {
                    throw new InvalidOperationException("No application statistics entries in the database");
                }

                SSIStatistics ssiStatisticsDb;
                SSIStatisticsVaccination ssiStatisticsDbVaccination;
                if (packageDate != null)
                {

                    bool success = DateTime.TryParse(packageDate, out DateTime lastPackageDate);
                    if (!success)
                    {
                        _logger.LogError("Could not parse package date");
                        return BadRequest("Could not parse package date");
                    }
                    ssiStatisticsDb = await _ssiIStatisticsRepository.GetEntryByDateAsync(lastPackageDate);
                    ssiStatisticsDbVaccination = await _ssiStatisticsVaccinationRepository.GetEntryByDateAsync(lastPackageDate);
                }
                else
                {
                    ssiStatisticsDb = await _ssiIStatisticsRepository.GetNewestEntryAsync();
                    ssiStatisticsDbVaccination = await _ssiStatisticsVaccinationRepository.GetNewestEntryAsync();
                }

                if (ssiStatisticsDb != null && ssiStatisticsDbVaccination != null)
                {
                    var resultsDbTuple =
                        new Tuple<SSIStatistics, SSIStatisticsVaccination, ApplicationStatistics>(ssiStatisticsDb,
                            ssiStatisticsDbVaccination, applicationStatisticsDb);
                    
                    var ssiStatisticsDto =
                        _mapper
                            .Map<Tuple<SSIStatistics, SSIStatisticsVaccination, ApplicationStatistics>,
                                CovidStatisticsDto>(resultsDbTuple);

                    return Ok(ssiStatisticsDto);
                }

                return NoContent();
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.ToString());
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected behaviour. Exception message: {e}");
                return StatusCode(500);
            }
        }
    }
}