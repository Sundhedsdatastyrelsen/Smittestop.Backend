﻿using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Core.Enums;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DIGNDB.App.SmitteStop.Core.Services
{
    public class PackageBuilderService : IPackageBuilderService
    {
        public int MaxKeysPerFile => _maxKeysPerFile;
        private readonly IConfiguration _configuration;
        private readonly ITemporaryExposureKeyRepository _temporaryExposureKeyRepository;
        private readonly IDatabaseKeysToBinaryStreamMapperService _databaseKeysToBinaryStreamMapperService;
        private readonly IKeysListToMemoryStreamConverter _keysListToMemoryStreamConverter;

        private readonly int _maxKeysPerFile;
        private readonly int _fetchCommandTimeout = 0;//set default value to 0 so the timeout is not changed unless a value is given in the config


        public PackageBuilderService(IDatabaseKeysToBinaryStreamMapperService databaseKeysToBinaryStreamMapperService, IConfiguration configuration, ITemporaryExposureKeyRepository temporaryExposureKeyRepository,
            IKeysListToMemoryStreamConverter keysListToMemoryStreamConverter)
        {
            _databaseKeysToBinaryStreamMapperService = databaseKeysToBinaryStreamMapperService;
            _configuration = configuration;
            _temporaryExposureKeyRepository = temporaryExposureKeyRepository;
            _keysListToMemoryStreamConverter = keysListToMemoryStreamConverter;
            if (!int.TryParse(_configuration["AppSettings:MaxKeysPerFile"], out _maxKeysPerFile))
            {
                _maxKeysPerFile = 750000;
            }
            int.TryParse(_configuration["AppSettings:FetchCommandTimeout"], out _fetchCommandTimeout);
        }

        public CacheResult BuildPackage(DateTime packageDate)
        {
            List<byte[]> streamBytesList = new List<byte[]>();

            //create file and store in cache if file does not exist in cache
            var keys = _temporaryExposureKeyRepository.GetTemporaryExposureKeysWithDkOrigin(packageDate, _fetchCommandTimeout);

            int skip = 0;
            while (keys.Count > skip)
            {
                var keysBatch = keys.Skip(skip).Take(_maxKeysPerFile);
                skip += _maxKeysPerFile;
                using (Stream stream = _databaseKeysToBinaryStreamMapperService.ExportDiagnosisKeys(keysBatch.ToList()))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(memoryStream);
                        stream.Seek(0, SeekOrigin.Begin);
                        streamBytesList.Add(memoryStream.ToArray());
                    }
                }
            }

            return new CacheResult()
            { FileBytesList = streamBytesList };
        }

        public List<byte[]> BuildPackageContentV2(DateTime startDate, ZipFileOrigin origin)
        {
            List<byte[]> streamBytesList = new List<byte[]>();

            bool allPackagesCreated = false;
            int numberOfRecordsToSkip = 0;
            while (!allPackagesCreated)
            {
                var keys = GetNextKeysBatch(origin, startDate, numberOfRecordsToSkip);
                numberOfRecordsToSkip += keys.Count();
                if (keys.Count != 0)
                {
                    streamBytesList.Add(_keysListToMemoryStreamConverter.ConvertKeysToMemoryStream(keys));
                }
                else
                {
                    allPackagesCreated = true;
                }
            }
            return streamBytesList;
        }

        private IList<TemporaryExposureKey> GetNextKeysBatch(ZipFileOrigin origin, DateTime startDate, int numberOfRecordsToSkip)
        {
            if (origin == ZipFileOrigin.All)
            {
                return _temporaryExposureKeyRepository.GetAllTemporaryExposureKeysForPeriodNextBatch(startDate, numberOfRecordsToSkip, _maxKeysPerFile);
            }
            else
            {
                return _temporaryExposureKeyRepository.GetDkTemporaryExposureKeysForPeriodNextBatch(startDate, numberOfRecordsToSkip, _maxKeysPerFile);
            }
        }
    }
}
