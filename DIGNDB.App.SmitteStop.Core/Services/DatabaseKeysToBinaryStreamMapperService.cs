using DIGNDB.App.SmitteStop.Core.Configs;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Core.Helpers;
using DIGNDB.App.SmitteStop.Domain.Db;
using System.Collections.Generic;
using System.IO;

namespace DIGNDB.App.SmitteStop.Core.Services
{
    public class DatabaseKeysToBinaryStreamMapperService : IDatabaseKeysToBinaryStreamMapperService
    {
        private readonly IExposureKeyMapper _exposureKeyMapper;
        private readonly TemporaryExposureKeyZipFilesSettings _config;

        public DatabaseKeysToBinaryStreamMapperService(IExposureKeyMapper exposureKeyMapper, TemporaryExposureKeyZipFilesSettings config)
        {
            _exposureKeyMapper = exposureKeyMapper;
            _config = config;
        }

        public Stream ExportDiagnosisKeys(IList<TemporaryExposureKey> keys)
        {
            var exportBatch = _exposureKeyMapper.FromEntityToProtoBatch(keys);
            var exportUtil = new ExposureBatchFileUtil(_config.CertificateThumbprint);
            var task = exportUtil.CreateSignedFileAsync(exportBatch);
            task.Wait();
            return task.Result;
        }
    }
}
