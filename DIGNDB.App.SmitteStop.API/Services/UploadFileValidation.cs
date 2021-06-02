using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.APP.SmitteStop.API.Config;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace DIGNDB.App.SmitteStop.API.Services
{
    /// <summary>
    /// File upload validation
    /// </summary>
    public class UploadFileValidationService : IUploadFileValidationService
    {
        private string _message = "";
        private readonly ApiConfig _configuration;
        private IFormFile _file;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        public UploadFileValidationService(ApiConfig configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Verifies that the uploaded file has the allowed size, name, and structure
        /// </summary>
        /// <param name="file"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Verify(IFormFile file, out string message)
        {
            _file = file;
            bool checkResult = true;

            checkResult &= CheckSize();
            checkResult &= CheckName();
            if (checkResult)
            {
                checkResult &= CheckExtension();
            }

            if (checkResult == false)
            {
                LogStructureOfFile();
            }

            message = _message;

            return checkResult;
        }

        private bool CheckSize()
        {
            var size = int.Parse(_configuration.UploadFileValidationRules.FileSize);
            if (_file.Length < 1 || _file.Length > size)
            {
                _message += $"File {_file.FileName} is empty or too large. Maximum size is {size}. ";
                return false;
            }
            return true;
        }

        private bool CheckName()
        {
            var infectionFileRegexPattern = _configuration.UploadFileValidationRules.InfectionFileRegexPattern;
            var vaccinationFileRegexPattern = _configuration.UploadFileValidationRules.VaccinationFileRegexPattern;

            Regex r = new Regex(infectionFileRegexPattern);
            var match =  r.IsMatch(_file.FileName);

            r = new Regex(vaccinationFileRegexPattern);
            match |= r.IsMatch(_file.FileName);

            if (match == false)
            {
                _message += $"File name {_file.FileName} does not match with allowed names . ";
                return false;
            }
            return true;
        }

        private bool CheckExtension()
        {
            Stream stream = _file.OpenReadStream();
            Stream memStream = new MemoryStream();
            stream.CopyTo(memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            using (ZipArchive archive = new ZipArchive(memStream))
            {
                if (archive.Entries.Count == 0)
                {
                    _message += $"Zip archive {_file.FileName} is empty. ";
                    return false;
                }

                var ignorePathList = _configuration.UploadFileValidationRules.PathNamesIgnoreByVerification.Split(",");

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (ignorePathList.Contains(entry.FullName)) // Only these three files/folders are accepted besides of .csv files
                    {
                        continue;
                    }
                    _message += $"Zip archive {_file.FileName} contain {entry.FullName} file.";
                    return false;
                }
            }

            return true;
        }

        private void LogStructureOfFile()
        {
            var stream = _file.OpenReadStream();
            var memStream = new MemoryStream();
            stream.CopyTo(memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            using var archive = new ZipArchive(memStream);
            _message += $"\n Structure of Zip file {_file.FileName}:";

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                _message += $"\n - {entry.FullName};";
            }

            _message += $"\n End. {_file.FileName}.";
            _message += "Validation rules:  \n - Only *.csv files \n - No extra folders.";
        }
    }
}
