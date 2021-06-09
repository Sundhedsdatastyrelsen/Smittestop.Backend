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

        private DateTime _zipDateSSIVaccination;
        private const int DeleteOldEntriesAfterDays = 30;

        public const int VaccinationEncoding1252 = 1252;
        public Encoding VaccinationEncodingUtf8 = Encoding.UTF8;

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

                var ssiStatisticsVaccination = ValidateAndBuildDatabaseEntryFromSsiVaccinationData(vaccinationPercentages);

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

                var ssiStatistics = SumStatisticsByColumn(statistics.Statistics);

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
            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(VaccinationEncoding1252);

            try
            {
                var record = GetVaccineRecord(zipEntry, classMap, encoding);
                return record;
            }
            catch (Exception e)
            {
                try
                {
                    encoding = VaccinationEncodingUtf8;
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

        private List<T> RetrieveDataFromZipEntry1252<T>(ZipArchiveEntry zipEntry, ClassMap<T> classMap)
        {
            try
            {
                var encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);
                var records = GetCsvEntriesFromZipFile(zipEntry, classMap, encoding);
                return records;
            }
            catch (Exception e1252)
            {
                try
                {
                    var encoding = Encoding.UTF8;
                    var records = GetCsvEntriesFromZipFile(zipEntry, classMap, encoding);
                    return records;
                }
                catch (Exception exUtf8)
                {
                    var errorMessage1252 = $"| Process SSI file job exception | first exception using encoding 1252:\n {e1252.Message} - {e1252.StackTrace}";
                    var errorMessageUtf8 = $"| Process SSI file job exception | second exception using encoding UTF8:\n {exUtf8.Message} - {exUtf8.StackTrace}";
                    var combined = new Exception($"{errorMessage1252}\n{errorMessageUtf8}");
                    throw new SSIZipFileParseException(
                        "Covid statistics: There was a problem when reading one of the excel files", combined);
                }
            }
        }

        private List<T> GetCsvEntriesFromZipFile<T>(ZipArchiveEntry zipEntry, ClassMap<T> classMap, Encoding encoding)
        {
            using var csvStream = zipEntry.Open();
            using var csvStreamReader = new StreamReader(csvStream, encoding);
            using var csvContentReader = new CsvReader(csvStreamReader, new CultureInfo(_config.Culture));
            csvContentReader.Configuration.RegisterClassMap(classMap);
            csvContentReader.Configuration.PrepareHeaderForMatch =
                (header, _) => Regex.Replace(header, @"\s", string.Empty);
            var records = csvContentReader.GetRecords<T>();
            var retVal = records.ToList();
            return retVal;
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

        private SSIStatisticsVaccination ValidateAndBuildDatabaseEntryFromSsiVaccinationData(SSIStatisticsCsvDto vaccinationData)
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