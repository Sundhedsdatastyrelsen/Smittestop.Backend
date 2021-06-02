using CsvHelper;
using CsvHelper.Configuration;
using DIGNDB.App.SmitteStop.Core.Adapters;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Jobs.Services.Interfaces;
using DIGNDB.APP.SmitteStop.Jobs.ClassMaps;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;
using DIGNDB.APP.SmitteStop.Jobs.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DIGNDB.APP.SmitteStop.Jobs.Services
{
    public class SSIZipFileReaderService : ISSIZipFileReaderService
    {
        private readonly ISSIStatisticsRepository _ssiStatisticsRepository;
        private readonly ISSIStatisticsVaccinationRepository _ssiStatisticsVaccinationRepository;
        private readonly ILoggerAdapter<SSIZipFileReaderService> _logger;
        private readonly SSIExcelParsingConfig _config;

        private DateTime _zipDateSSIInfection;
        private DateTime _zipDateSSIVaccination;
        private const int DeleteOldEntriesAfterDays = 30;

        public const int VaccinationEncoding = 1252;

        public SSIZipFileReaderService(ISSIStatisticsRepository ssiStatisticsRepository, ISSIStatisticsVaccinationRepository ssiStatisticsVaccinationRepository, SSIExcelParsingConfig config, ILoggerAdapter<SSIZipFileReaderService> logger)
        {
            _logger = logger;
            _config = config;
            _ssiStatisticsRepository = ssiStatisticsRepository;
            _ssiStatisticsVaccinationRepository = ssiStatisticsVaccinationRepository;
        }

        public void HandleSsiVaccinationZipArchive(SSIZipArchivesInfoDto zipArchivesInfo)
        {
            try
            {
                if (zipArchivesInfo?.VaccinationArchive == null)
                {
                    throw new ArgumentException("Passed null argument. Cannot handle vaccination zip archive");
                }

                _logger.LogInformation($"SSI statistics: started processing of the vaccination zip file. Newer zip file creation date: {zipArchivesInfo.DateInfection}");
                _zipDateSSIVaccination = zipArchivesInfo.DateVaccination;

                var vaccinationPercentages = GetDataFromVaccinationZipArchive(zipArchivesInfo.VaccinationArchive);
                
                _ssiStatisticsVaccinationRepository.RemoveEntriesOlderThan(DateTime.UtcNow.Date.AddDays(-DeleteOldEntriesAfterDays));

                var ssiStatisticsVaccination = ValidateAndBuildDatabaseEntryFromSSIVaccinationData(vaccinationPercentages);

                var existingSsiStatistics = _ssiStatisticsVaccinationRepository.GetEntryByDate(ssiStatisticsVaccination.Date);
                if (existingSsiStatistics != null)
                {
                    _logger.LogInformation($"SSI statistics: Entry already exists for this date. Updating the entry.");
                    _ssiStatisticsVaccinationRepository.Delete(existingSsiStatistics);
                }

                _ssiStatisticsVaccinationRepository.CreateEntry(ssiStatisticsVaccination);
                _logger.LogInformation(
                    $"SSI infection statistics: file parsing completed. Values inserted {ssiStatisticsVaccination.VaccinationFirst} and {ssiStatisticsVaccination.VaccinationSecond}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
            _logger.LogInformation("Covid statistics: successfully parsed the file and saved results to the database");
        }

        public void HandleSsiStatisticsZipArchive(SSIZipArchivesInfoDto zipArchivesInfo)
        {
            try
            {
                if (zipArchivesInfo?.StatisticsArchive == null)
                {
                    throw new ArgumentException("Passed null argument. Cannot handle statistics zip archive");
                }

                _logger.LogInformation($"SSI statistics: started processing of the statistics zip file.");

                var statistics = GetDataFromStstisticsZipArchive(zipArchivesInfo.StatisticsArchive);

                _ssiStatisticsRepository.RemoveEntriesOlderThan(DateTime.UtcNow.Date.AddDays(-DeleteOldEntriesAfterDays));

                SSIStatistics ssiStatistics = SumStatisticsByColumn(statistics.Statistics);

                ssiStatistics.Date = zipArchivesInfo.DateInfection;
                var existingSsiStatistics = _ssiStatisticsRepository.GetEntryByDate(ssiStatistics.Date);
                if (existingSsiStatistics != null)
                {
                    _logger.LogInformation($"SSI statistics: Entry already exists for this date. Updating the entry.");
                    _ssiStatisticsRepository.Delete(existingSsiStatistics);
                }

                _ssiStatisticsRepository.CreateEntry(ssiStatistics);
                _logger.LogInformation($"SSI infection statistics: file parsing completed");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
            _logger.LogInformation("Covid statistics: successfully parsed the file and saved results to the database");
        }

        private SSIStatistics SumStatisticsByColumn(List<StatisticsDto> statistics)
        {
            SSIStatistics ssiStatistics = new SSIStatistics()
            {
                ConfirmedCasesToday = 0,
                ConfirmedCasesTotal = 0,
                DeathsToday = 0,
                DeathsTotal = 0,
                TestsConductedToday = 0,
                TestsConductedTotal = 0,
            };
            foreach (StatisticsDto statisticsDto in statistics)
            {
                ssiStatistics.ConfirmedCasesToday += statisticsDto.ChangedConfirmedCases;
                ssiStatistics.ConfirmedCasesTotal += statisticsDto.ConfirmedCases;
                ssiStatistics.DeathsToday += statisticsDto.ChangedDied;
                ssiStatistics.DeathsTotal += statisticsDto.Died;
                ssiStatistics.TestsConductedToday += statisticsDto.ChangedNumberSamples;
                ssiStatistics.TestsConductedTotal += statisticsDto.NumberSamples;
            }

            
            return ssiStatistics;
        }
        private SSIStatisticsCsvDto GetDataFromInfectionZipArchive(ZipArchive zipArchive)
        {
            SSIZipContentDto zipContent = PopulateInfectionZipContent(zipArchive);
            return RetrieveDataFromInfectionZipContent(zipContent);
        }

        private SSIStatisticsCsvDto GetDataFromVaccinationZipArchive(ZipArchive zipArchive)
        {
            SSIZipContentDto zipContent = PopulateVaccinationZipContent(zipArchive);
            return RetrieveDataFromVaccinationZipContent(zipContent);
        }

        private SSIStatisticsCsvDto GetDataFromStstisticsZipArchive(ZipArchive zipArchive)
        {
            SSIZipContentDto zipContent = PopulateStatisticsZipContent(zipArchive);
            return RetrieveDataFromStatisticsZipContent(zipContent);
        }

        private SSIZipContentDto PopulateInfectionZipContent(ZipArchive zipArchive)
        {
            var zipContent = new SSIZipContentDto();
            try
            {
                zipContent.DeathsOverTime = zipArchive.Entries.Single(x => x.FullName == _config.DeathsOverTime.FileName);
                zipContent.NewlyAdmittedOverTime = zipArchive.Entries.Single(x => x.FullName == _config.NewlyAdmittedOverTime.FileName);
                zipContent.TestPosOverTime = zipArchive.Entries.Single(x => x.FullName == _config.TestPosOverTime.FileName);
                zipContent.Tested = zipArchive.Entries.Single(x => x.FullName == _config.Tested.FileName);
            }
            catch (InvalidOperationException e)
            {
                throw new SSIZipFileParseException("Covid statistics: One of the excel files is missing in zip package for infection numbers", e);
            }

            return zipContent;
        }

        private SSIZipContentDto PopulateVaccinationZipContent(ZipArchive zipArchive)
        {
            var zipContent = new SSIZipContentDto();
            try
            {
                zipContent.Vaccinated = zipArchive.Entries.Single(x => x.FullName == _config.Vaccinated.FileName);
            }
            catch (InvalidOperationException e)
            {
                throw new SSIZipFileParseException($"Covid statistics: The vaccination percentages excel file {_config.Vaccinated.FileName} is missing in zip package", e);
            }

            return zipContent;
        }

        private SSIZipContentDto PopulateStatisticsZipContent(ZipArchive zipArchive)
        {
            var zipContent = new SSIZipContentDto();
            try
            {
                zipContent.Statistics = zipArchive.Entries.Single(x => x.FullName == _config.CovidStatistics.FileName);
            }
            catch (InvalidOperationException e)
            {
                throw new SSIZipFileParseException($"Covid statistics: The vaccination percentages excel file {_config.CovidStatistics.FileName} is missing in zip package", e);
            }

            return zipContent;
        }

        private T RetrieveVaccinationDataFromZipEntry<T>(ZipArchiveEntry zipEntry, ClassMap<T> classMap)
        {
            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(VaccinationEncoding);

            try
            {
                var record = GetVaccineRecord(zipEntry, classMap, encoding);
                return record;
            }
            catch (Exception e)
            {
                try
                {
                    encoding = Encoding.UTF8;
                    var record = GetVaccineRecord(zipEntry, classMap, encoding);
                    return record;

                }
                catch (Exception)
                {
                    var errorMessage = "Covid statistics: There was a problem reading vaccine numbers from the excel files";
                    _logger.LogError(errorMessage);

                    throw new SSIZipFileParseException(errorMessage, e);

                }
            }
        }

        private T GetVaccineRecord<T>(ZipArchiveEntry zipEntry, ClassMap<T> classMap, Encoding encoding)
        {
            using var csvStream = zipEntry.Open();
            // Encoding based on file from https://covid19.ssi.dk/overvagningsdata/download-fil-med-vaccinationsdata
            using var csvStreamReader = new StreamReader(csvStream, encoding);
            var config = new CsvConfiguration(new CultureInfo(_config.Vaccinated.VaccinationCulture, false));
            config.RegisterClassMap(classMap);
            config.PrepareHeaderForMatch = (header, _) => Regex.Replace(header, @"\s", string.Empty);

            using var csv = new CsvReader(csvStreamReader, config);
            csv.Read();
            csv.ReadHeader();
            csv.Read();
            var record = csv.GetRecord<T>();
            return record;
        }

        private List<T> RetrieveDataFromZipEntry<T>(ZipArchiveEntry zipEntry, ClassMap<T> classMap)
        {
            try
            {
                using var csvStream = zipEntry.Open();
                using var csvStreamReader = new StreamReader(csvStream);
                using var csvContentReader = new CsvReader(csvStreamReader, new CultureInfo(_config.Culture));
                csvContentReader.Configuration.RegisterClassMap(classMap);
                csvContentReader.Configuration.PrepareHeaderForMatch =
                    (header, _) => Regex.Replace(header, @"\s", string.Empty);
                List<T> records = csvContentReader.GetRecords<T>().ToList();
                return records;
            }
            catch (Exception e)
            {
                throw new SSIZipFileParseException(
                    "Covid statistics: There was a problem when reading one of the excel files", e);
            }
        }

        private List<T> RetrieveDataFromZipEntry1252<T>(ZipArchiveEntry zipEntry, ClassMap<T> classMap)
        {
            try
            {
                using var csvStream = zipEntry.Open();
                using var csvStreamReader =
                    new StreamReader(csvStream, CodePagesEncodingProvider.Instance.GetEncoding(1252));
                using var csvContentReader = new CsvReader(csvStreamReader, new CultureInfo(_config.Culture));
                csvContentReader.Configuration.RegisterClassMap(classMap);
                csvContentReader.Configuration.PrepareHeaderForMatch =
                    (header, _) => Regex.Replace(header, @"\s", string.Empty);
                List<T> records = csvContentReader.GetRecords<T>().ToList();
                return records;
            }
            catch (Exception e)
            {
                try
                {
                    using var csvStream = zipEntry.Open();
                    using var csvStreamReader =
                        new StreamReader(csvStream, Encoding.UTF8);
                    using var csvContentReader = new CsvReader(csvStreamReader, new CultureInfo(_config.Culture));
                    csvContentReader.Configuration.RegisterClassMap(classMap);
                    csvContentReader.Configuration.PrepareHeaderForMatch =
                        (header, _) => Regex.Replace(header, @"\s", string.Empty);
                    List<T> records = csvContentReader.GetRecords<T>().ToList();
                    return records;
                }

                catch (Exception ex)
                {
                    throw new SSIZipFileParseException(
                        "Covid statistics: There was a problem when reading one of the excel files", ex);
                }
            }
        }
    

        private SSIStatisticsCsvDto RetrieveDataFromInfectionZipContent(SSIZipContentDto todayZipContent)
        {
            var deathsOverTimeDtoMap = new DeathsOverTimeDtoMap(_config);
            var newlyAdmittedOverTimeDtoMap = new NewlyAdmittedOverTimeDtoMap(_config);
            var testPosOverTimeDtoMap = new TestPosOverTimeDtoMap(_config);
            var testRegionerDtoMap = new TestedExcelDtoMap(_config);
            return new SSIStatisticsCsvDto
            {
                DeathsOverTime = RetrieveDataFromZipEntry(todayZipContent.DeathsOverTime, deathsOverTimeDtoMap),
                NewlyAdmittedOverTime = RetrieveDataFromZipEntry(todayZipContent.NewlyAdmittedOverTime, newlyAdmittedOverTimeDtoMap),
                TestPosOverTime = RetrieveDataFromZipEntry(todayZipContent.TestPosOverTime, testPosOverTimeDtoMap),
                TestedExcel = RetrieveDataFromZipEntry(todayZipContent.Tested, testRegionerDtoMap)
            };
        }

        private SSIStatisticsCsvDto RetrieveDataFromVaccinationZipContent(SSIZipContentDto todayZipContent)
        {
            var vaccinationPercentagesDtoMap = new VaccinationPercentagesDtoMap(_config);
            var data = RetrieveVaccinationDataFromZipEntry(todayZipContent.Vaccinated, vaccinationPercentagesDtoMap);

            var retVal = new SSIStatisticsCsvDto
            {
                VaccinationPercentages = data
            };

            _logger.LogInformation(
                $"Vaccination numbers read from CSV file: '{retVal.VaccinationPercentages.FirstTime}' and '{retVal.VaccinationPercentages.SecondTime}'");

            return retVal;
        }

        private SSIStatisticsCsvDto RetrieveDataFromStatisticsZipContent(SSIZipContentDto todayZipContent)
        {
            var statisticsDtoMap = new StatisticsDtoMap(_config);
            return new SSIStatisticsCsvDto
            {
                Statistics = RetrieveDataFromZipEntry1252(todayZipContent.Statistics, statisticsDtoMap)
            };
        }

        private SSIStatistics ValidateAndBuildDatabaseEntryFromSSIInfectionData(SSIStatisticsCsvDto todayData, SSIStatisticsCsvDto yesterdayData)
        {
            try
            {
                var todayTotalDeaths = todayData.DeathsOverTime
                    .Where(x => _config.TotalColumnNames.Contains(x.DateString)).Select(x => x.Deaths).Single();
                var yesterdayTotalDeaths = yesterdayData.DeathsOverTime
                    .Where(x => _config.TotalColumnNames.Contains(x.DateString)).Select(x => x.Deaths).Single();
                var todayTotalTested = todayData.TestedExcel
                    .Where(x => _config.TotalColumnNames.Contains(x.DateString)).Select(x => x.Tested).Single();
                var yesterdayTotalTested = yesterdayData.TestedExcel
                    .Where(x => _config.TotalColumnNames.Contains(x.DateString)).Select(x => x.Tested).Single();
                var todayTotalConfirmedCases = todayData.TestPosOverTime
                    .Where(x => _config.TotalColumnNames.Contains(x.DateString)).Select(x => x.ConfirmedCases).Single();
                var yesterdayTotalConfirmedCases = yesterdayData.TestPosOverTime
                    .Where(x => _config.TotalColumnNames.Contains(x.DateString)).Select(x => x.ConfirmedCases).Single();
                var todayAdmitted = todayData.NewlyAdmittedOverTime.Last().Hospitalized;

                var atLeastOneCellIsMissingData =
                    todayTotalConfirmedCases == -1 || yesterdayTotalConfirmedCases == -1 || todayTotalDeaths == -1 ||
                    yesterdayTotalDeaths == -1 || todayTotalTested == -1 || yesterdayTotalTested == -1 ||
                    todayAdmitted == -1;
                
                if (atLeastOneCellIsMissingData)
                {
                    throw new SSIZipFileParseException(
                        "Covid statistics: Detected missing data in key cells used to calculate the results");
                }

                var ssiStatistics = new SSIStatistics
                {
                    ConfirmedCasesToday = todayTotalConfirmedCases - yesterdayTotalConfirmedCases,
                    ConfirmedCasesTotal = todayTotalConfirmedCases,
                    DeathsToday = todayTotalDeaths - yesterdayTotalDeaths,
                    DeathsTotal = todayTotalDeaths,
                    TestsConductedToday = todayTotalTested - yesterdayTotalTested,
                    TestsConductedTotal = todayTotalTested,
                    //PatientsAdmittedToday = todayAdmitted,
                    Date = _zipDateSSIInfection
                };

                return ssiStatistics;
            }
            catch (InvalidOperationException e)
            {
                throw new SSIZipFileParseException(
                    "Covid statistics: Could not find entries used to build the statistics infection object in the database: " +
                    "Could not find a string that indicate \"total\" column", e);
            }
        }

        private SSIStatisticsVaccination ValidateAndBuildDatabaseEntryFromSSIVaccinationData(SSIStatisticsCsvDto vaccinationData)
        {
            try
            {
                var vaccinatedFirstTime = vaccinationData.VaccinationPercentages.FirstTime;
                var vaccinatedSecondTime = vaccinationData.VaccinationPercentages.SecondTime;

                if (!(vaccinatedFirstTime > 0.0))
                {
                    throw new SSIZipFileParseException(
                        "Covid statistics: number for vaccination first time is 0");
                }

                if (!(vaccinatedSecondTime > 0.0))
                {
                    throw new SSIZipFileParseException(
                        "Covid statistics: number for vaccination second time is 0");
                }

                var ssiStatistics = new SSIStatisticsVaccination
                {
                    Date = _zipDateSSIVaccination,
                    VaccinationFirst = vaccinatedFirstTime,
                    VaccinationSecond = vaccinatedSecondTime
                };

                return ssiStatistics;
            }
            catch (InvalidOperationException e)
            {
                throw new SSIZipFileParseException(
                    "Covid statistics: Could not find entries used to build the statistics vaccination object in the database", e);
            }
        }
    }
}