using DIGNDB.App.SmitteStop.Core.Adapters;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Jobs.Services.Interfaces;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;
using DIGNDB.APP.SmitteStop.Jobs.Exceptions;
using DIGNDB.APP.SmitteStop.Jobs.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DIGNDB.App.SmitteStop.Jobs.Services
{
    public class SSIZipFolderHandlingService : ISSIZipFolderHandlingService
    {
        private readonly ILoggerAdapter<SSIZipFolderHandlingService> _logger;
        private readonly SSIZipFolderProcessingConfig _config;
        private readonly IFileSystem _fileSystem;

        public SSIZipFolderHandlingService(SSIZipFolderProcessingConfig config, ILoggerAdapter<SSIZipFolderHandlingService> logger, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _config = config;
        }

        public SSIZipArchivesInfoDto GetNewestArchivesFromFolder()
        {
            _logger.LogInformation("Started processing SSI statistics directory");
            try
            {
                string directoryPath = _config.StatisticsZipFolderPath;
                bool directoryExists = _fileSystem.DirectoryExists(directoryPath);
                if (directoryExists)
                {
                    var zipArchivesInfo = ProcessDirectory(directoryPath);
                    return zipArchivesInfo;
                }
                else
                {
                    throw new SsiZipFolderProcessingException(
                        $"Covid statistics: Could not find directory specified for covid statistics. Directory = {_config.StatisticsZipFolderPath}");
                }
            }
            catch (SsiZipFolderProcessingException e)
            {
                _logger.LogError($"{e}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected error occurred while processing covid statistics. Error message : {e}");
                throw;
            }
        }

        private SSIZipArchivesInfoDto ProcessDirectory(string directoryPath)
        {
            var fileNames = _fileSystem.GetFileNamesFromDirectory(directoryPath).ToList();
            var directoryContainsOnlyZipFiles = FileHelper.DoesFileNamesContainOnlyZips(fileNames);

            if (!directoryContainsOnlyZipFiles)
            {
                const string errorMessage = "Covid statistics: Specified directory with covid statistics is corrupted - it does not contain only zip files.";
                _logger.LogError(errorMessage);
                throw new SsiZipFolderProcessingException(errorMessage);

            }

            SSIZipArchivesInfoDto ssiZipArchives = new SSIZipArchivesInfoDto();
            
            AddVaccinationArchive(fileNames, ref ssiZipArchives);

            AddStatisticsArchive(fileNames, ref ssiZipArchives);

            return ssiZipArchives;
        }

        private void AddVaccinationArchive(List<string> fileNames, ref SSIZipArchivesInfoDto ssiZipArchives)
        {
            var vaccinationFileNames = fileNames.Where(f => f.Contains(_config.VaccinationNumbersPrefix)).ToList();
            DisplayWarningIfSomeFilesDoesNotContainParsableDate(vaccinationFileNames,
                _config.ZipPackageVaccineDatePattern, _config.ZipPackageVaccineDateParsingFormat);
            SortVaccinationFileNamesAccordingToExtractedDate(vaccinationFileNames);

            var todaysVaccinationNumbers = _fileSystem.OpenZip(vaccinationFileNames[0]);
            ssiZipArchives.VaccinationArchive = todaysVaccinationNumbers;
            ssiZipArchives.DateVaccination = _fileSystem.GetCreationDateUTC(vaccinationFileNames[0]);
        }

        private void AddStatisticsArchive(List<string> fileNames, ref SSIZipArchivesInfoDto ssiZipArchives)
        {
            var statisticsFileNames = fileNames.Where(f => f.Contains(_config.StatisticsPrefix)).ToList();
            DisplayWarningIfSomeFilesDoesNotContainParsableDate(statisticsFileNames,
                 _config.ZipPackageVaccineDatePattern, _config.ZipPackageVaccineDateParsingFormat);
            SortVaccinationFileNamesAccordingToExtractedDate(statisticsFileNames);

            if (statisticsFileNames.Count > 0)
            {
                var statisticsArchive = _fileSystem.OpenZip(statisticsFileNames[0]);
                ssiZipArchives.StatisticsArchive = statisticsArchive;
                ssiZipArchives.DateInfection = _fileSystem.GetCreationDateUTC(statisticsFileNames[0]);
            }
        }
        
        private void SortVaccinationFileNamesAccordingToExtractedDate(List<string> fileNames)
        {
            fileNames.Sort((value1, value2) =>
                FilenameParsingHelper.FilenameWithExtractionDateComparer(value1, value2, _config.ZipPackageVaccineDatePattern,
                    _config.ZipPackageVaccineDateParsingFormat));
            fileNames.Reverse();
        }

        private void DisplayWarningIfSomeFilesDoesNotContainParsableDate(List<string> fileNames, string zipPackageDatePattern, string zipPackageDateParsingFormat)
        {
            try
            {
                foreach (var filename in fileNames)
                {
                    FilenameParsingHelper.ExtractDateFromFilename(filename, zipPackageDatePattern,
                        zipPackageDateParsingFormat);
                }
            }
            catch (SsiZipFolderProcessingException)
            {
                _logger.LogWarning(
                    "Covid statistics: Could not parse date in at lease one of the files that are present in the trifork integration folder");
            }
        }
    }
}