﻿using DIGNDB.App.SmitteStop.Core.Configs;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Core.Helpers;
using DIGNDB.App.SmitteStop.Core.Services;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest
{
    [TestFixture]
    public class DiagnosisKeysServiceTests
    {
        private TemporaryExposureKeyZipFilesSettings _settings;
        private IEpochConverter _epochConverter;
        private ExposureKeyMapper _mapper;
        private readonly string _pemKeyFromGoogle = @"-----BEGIN EC PRIVATE KEY-----
MHcCAQEEIE7yE32GaV/+qZ2tlOpdZRIXc9SJsyT5QhDJd9njZ+kcoAoGCCqGSM49
AwEHoUQDQgAEml59itec9qzwVojreLXdPNRsUWzfYHc1cKvIIi6/H56AJS/kZEYQ
nfDpxrgyGhdAm+pNN2GAJ3XdnQZ1Sk4amg==
-----END EC PRIVATE KEY-----";

        private readonly string _pemFilePath = string.Format("{0}{1}", System.IO.Directory.GetCurrentDirectory().Split("\\bin")[0], "\\test.pem");

        [SetUp]
        public void Init()
        {
            CreatePemFile();
            _epochConverter = new EpochConverter();
            _mapper = new ExposureKeyMapper(_epochConverter);
            _settings = new TemporaryExposureKeyZipFilesSettings
            {
                CertificateThumbprint = _pemFilePath
            };
        }

        private void CreatePemFile()
        {
            File.WriteAllText(_pemFilePath, _pemKeyFromGoogle);
        }

        private IList<TemporaryExposureKey> CreateMockedListExposureKeys(DateTime expectDate)
        {
            var data = new List<TemporaryExposureKey> {
                new TemporaryExposureKey()
                {
                    CreatedOn = expectDate.Date,
                    Id = Guid.NewGuid(),
                    KeyData = Encoding.ASCII.GetBytes("keyData1"),
                    TransmissionRiskLevel = RiskLevel.RISK_LEVEL_LOW,
                },
                new TemporaryExposureKey()
                {
                    CreatedOn = expectDate.Date.AddDays(-12),
                    Id = Guid.NewGuid(),
                    KeyData = Encoding.ASCII.GetBytes("keyData2"),
                    TransmissionRiskLevel = RiskLevel.RISK_LEVEL_HIGH,
                }
            };
            return data;
        }

        [Test]
        public void ExportDiagnosisKeys_HaveKeys_ShouldReturnStream()
        {
            DatabaseKeysToBinaryStreamMapperService toBinaryStreamMapperService = new DatabaseKeysToBinaryStreamMapperService(_mapper, _settings);
            var expectDate = DateTime.UtcNow;
            var data = CreateMockedListExposureKeys(expectDate);
            var streamResult = toBinaryStreamMapperService.ExportDiagnosisKeys(data);
            Assert.AreNotEqual(0, streamResult.Length);
        }

        [Test]
        public void ExportDiagnosisKeys_HaveNoKey_ShouldThrowException()
        {
            DatabaseKeysToBinaryStreamMapperService toBinaryStreamMapperService = new DatabaseKeysToBinaryStreamMapperService(_mapper, _settings);
            var data = new List<TemporaryExposureKey> { };
            var exception = Assert.Throws<InvalidOperationException>(() => toBinaryStreamMapperService.ExportDiagnosisKeys(data));
            Assert.AreEqual(exception.Message, "Sequence contains no elements");
        }
    }
}
