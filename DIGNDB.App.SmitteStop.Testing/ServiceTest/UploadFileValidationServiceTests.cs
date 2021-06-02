using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.APP.SmitteStop.API.Config;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest
{
    [TestFixture]
    public class UploadFileValidationServiceTests
    {
        private IUploadFileValidationService _validationService;

        [SetUp]
        public void Init()
        {
            _validationService = new UploadFileValidationService(
                new ApiConfig()
                {
                    UploadFileValidationRules = new UploadFileValidationRules()
                    {
                        FileSize = "4000",
                        InfectionFileRegexPattern = "^Smittestop_smittetal_\\d{4}_\\d{2}_\\d{2}.zip$",
                        VaccinationFileRegexPattern = "^smittestop_vacc_COVID19_ArcGIS_\\d{4}_\\d{2}_\\d{2}.zip$",
                        PathNamesIgnoreByVerification = "0_read_me.txt,Vaccine_DB/,Vaccine_DB/0_readme_fil_vaccinationsDB.txt"
                    }
                }
                );
        }

        public IFormFile MakeZipFile(int numberOfFiles, bool emptyFiles, string fileName = "Smittestop_smittetal_2020_12_15.zip", string fileNameInternal = "test.csv")
        {
            var mockedFileCollection = new FormFileCollection();
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                if (!emptyFiles)
                {
                    var demoFile = archive.CreateEntry(fileNameInternal);

                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("Body of CSV file!");
                    }
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < numberOfFiles; i++)
            {
                mockedFileCollection.Add(new FormFile(memoryStream, 0, memoryStream.Length, "Data", fileName));
            }

            return mockedFileCollection.FirstOrDefault();
        }

        [Test]
        public void Verify_EverythingOK_Infection_Pattern()
        {
            var res = _validationService.Verify(MakeZipFile(1,false), out string str);
            Assert.AreEqual(true,res);
        }

        [Test]
        public void Verify_EverythingOK_Vaccination_Pattern()
        {
            var res = _validationService.Verify(MakeZipFile(1, false, "smittestop_vacc_COVID19_ArcGIS_2021_01_01.zip"), out string str);
            Assert.AreEqual(true, res);
        }

        [Test]
        public void Verify_EmptyZip()
        {
            var res = _validationService.Verify(MakeZipFile(1, true), out string str);
            Assert.AreEqual(false, res);
        }

        [Test]
        public void Verify_WrongExtenstion()
        {
            var res = _validationService.Verify(MakeZipFile(1, false, fileNameInternal: "test.txt"), out string str);
            Assert.AreEqual(false, res);
        }

        [Test]
        public void Verify_Ignore_File()
        { 
            var res = _validationService.Verify(MakeZipFile(1, false, fileNameInternal: "Vaccine_DB/"), out string str);
            Assert.AreEqual(true, res);
        }

        [Test]
        public void Verify_WrongName()
        {
            var res = _validationService.Verify(MakeZipFile(1, false, "Test_smittetal_2020_12_15.zip"), out string str);
            Assert.AreEqual(false, res);
        }

        [Test]
        public void Verify_WrongSize()
        {
            _validationService = new UploadFileValidationService(
                new ApiConfig()
                {
                    UploadFileValidationRules = new UploadFileValidationRules()
                    {
                        FileSize = "1",
                        InfectionFileRegexPattern = "^Smittestop_smittetal_\\d{4}_\\d{2}_\\d{2}.zip$",
                        VaccinationFileRegexPattern = "^smittestop_vacc_COVID19_ArcGIS_\\d{4}_\\d{2}_\\d{2}.zip$",
                        PathNamesIgnoreByVerification = "0_read_me.txt,Vaccine_DB/,Vaccine_DB/0_readme_fil_vaccinationsDB.txt"
                    }
                }
            );
            var res = _validationService.Verify(MakeZipFile(1, false), out string str);
            Assert.AreEqual(false, res);
        }
    }
}