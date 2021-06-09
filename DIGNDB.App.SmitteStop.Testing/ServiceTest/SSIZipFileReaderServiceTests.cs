using DIGNDB.App.SmitteStop.Core.Adapters;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Dto;
using DIGNDB.APP.SmitteStop.Jobs.Exceptions;
using DIGNDB.APP.SmitteStop.Jobs.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest
{
    [TestFixture]
    public class SSIZipFileReaderServiceTests
    {
        private MockRepository _mockRepository;
        private Mock<ISSIStatisticsRepository> _mockSSIStatisticsRepository;
        private Mock<ISSIStatisticsVaccinationRepository> _mockSSIStatisticsVaccinationRepository;
        private SSIExcelParsingConfig _mockSSIExcelParsingConfig;

        private Mock<ILoggerAdapter<SSIZipFileReaderService>> _mockLogger;

        private List<Tuple<string, string[]>> _sampleFilesToday;
        private List<Tuple<string, string[]>> _sampleFilesYesterday;
        private List<Tuple<string, string[]>> _sampleFilesVaccination;
        private List<Tuple<string, string[]>> _sampleFilesStatistics;

        private readonly DateTime _sampleDateInfection = new DateTime(2020, 10, 11, 3, 4, 5);
        private readonly DateTime _sampleDateVaccination = new DateTime(2020, 10, 11, 3, 4, 5);

        private SSIStatistics _expectedOutputInfection;
        private SSIStatistics _actualOutputInfection;

        private SSIStatisticsVaccination _expectedOutputVaccination;
        private SSIStatisticsVaccination _actualOutputVaccination;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _mockSSIStatisticsRepository = _mockRepository.Create<ISSIStatisticsRepository>();
            _mockSSIStatisticsVaccinationRepository = _mockRepository.Create<ISSIStatisticsVaccinationRepository>();

            _mockLogger = _mockRepository.Create<ILoggerAdapter<SSIZipFileReaderService>>(MockBehavior.Loose);
            SetupInfectionExampleOutput();
            SetupSampleFiles();
            SetupSampleConfig();
            SetupMocks();
        }

        private void SetupMocks()
        {
            _mockSSIStatisticsRepository.Setup(x => x.RemoveEntriesOlderThan(It.IsAny<DateTime>())).Verifiable();
            _mockSSIStatisticsRepository.Setup(x => x.CreateEntry(It.IsAny<SSIStatistics>()))
                .Callback<SSIStatistics>(x => _actualOutputInfection = x);

            _mockSSIStatisticsVaccinationRepository.Setup(x => x.RemoveEntriesOlderThan(It.IsAny<DateTime>())).Verifiable();
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.CreateEntry(It.IsAny<SSIStatisticsVaccination>()))
                .Callback<SSIStatisticsVaccination>(x => _actualOutputVaccination = x);
        }

        private void SetupSampleConfig()
        {
            _mockSSIExcelParsingConfig = new SSIExcelParsingConfig
            {
                Tested = new TestedExcelFileConfig
                {
                    FileName = "Test_regioner.csv",
                    TestedColumnNames = new[] { "Total" }
                },
                TotalColumnNames = new[] { "I alt", "Total" },
                Culture = "da-DK",
                DateColumnNames = new[] { "Dato", "Date", "ugenr" },
                DeathsOverTime = new DeathsOverTimeConfig()
                {
                    DeathsColumnNames = new[] { "Antal_døde" },
                    FileName = "Deaths_over_time.csv"
                },
                NewlyAdmittedOverTime = new NewlyAdmittedOverTimeConfig()
                {
                    FileName = "Newly_admitted_over_time.csv",
                    HospitalizedColumnNames = new[] { "Total" }
                },
                TestPosOverTime = new TestPosOverTimeConfig()
                {
                    FileName = "Test_pos_over_time.csv",
                    ConfirmedCasesColumnNames = new[] { "NewPositive" }
                },
                Vaccinated = new VaccinatedExcelFileConfig()
                {
                    FileName = "Vaccine_DB/Vaccinationsdaekning_nationalt.csv",
                    VaccinatedFirstTimeColumnName = "Vacc.dækning påbegyndt vacc. (%)",
                    VaccinatedSecondTimeColumnName = "Vacc.dækning faerdigvacc. (%)",
                    VaccinationCulture = "en-US"
                },
                CovidStatistics = new CovidStatistics()
                {
                    FileName = "Regionalt_DB/01_noegle_tal.csv",
                    ColumnNames = new[] { "Bekræftede tilfælde", "Døde", "Ændring i antal bekræftede tilfælde", "Ændring i antal døde", "Antallet af prøver", "Ændring i antallet af prøver" }
                }
            };
        }

        private void SetupInfectionExampleOutput()
        {
            _expectedOutputInfection = new SSIStatistics
            {
                ConfirmedCasesToday = 814,
                TestsConductedTotal = 21074483,
                ConfirmedCasesTotal = 225844,
                Date = _sampleDateInfection,
                DeathsToday = 5,
                DeathsTotal = 2409,
                TestsConductedToday = 187414
            };

            _expectedOutputVaccination = new SSIStatisticsVaccination
            {
                Date = _sampleDateVaccination,
                VaccinationFirst = 4.3,
                VaccinationSecond = 2.1
            };
        }

        private void SetupSampleFiles()
        {
            // Infection
            _sampleFilesToday = new List<Tuple<string, string[]>>();
            _sampleFilesYesterday = new List<Tuple<string, string[]>>();
            
            string[] todayDeathsOverTimeContent = new string[]
            {
                "Dato;Antal_døde",
                "2020-03-11; 1",
                $"I alt; {_expectedOutputInfection.DeathsTotal}"
            };
            string[] yesterdayDeathsOverTimeContent = new string[]
            {
                "Dato;Antal_døde",
                "2020-03-11; 1",
                $"I alt; {_expectedOutputInfection.DeathsTotal - _expectedOutputInfection.DeathsToday}"
            };
            string[] todayNewlyAdmittedOverTimeContent = new string[]
            {
                "Dato; Hovedstaden; Sjælland; Syddanmark; Midtjylland; Nordjylland; Ukendt Region; Total",
                "2020 - 03 - 01; 1; 0; 0; 0; 0; 0; 1",
                $"2020-12-06;39;11; 4;10; 0;0;"
            };
            string[] yesterdayNewlyAdmittedOverTimeContent = new string[]
            {
                "Dato; Hovedstaden; Sjælland; Syddanmark; Midtjylland; Nordjylland; Ukendt Region; Total",
                "2020 - 03 - 01; 1; 0; 0; 0; 0; 0; 1",
                $"2020-12-06;39;11; 4;10; 0;0;"
            };
            string[] todayTestsRegionerContent = new string[]
            {
                "ugenr; Region Hovedstaden; Region Midtjylland; Region Nordjylland; Region Sjælland; Region Syddanmark; Statens Serum Institut; Testcenter Danmark; Total; Kumulativ_total",
                "Uge 5; 1; 0; 0; 0; 0; 2; 0; 3; 3",
                $"Total; 1204000; 581774; 278812; 381410; 652733; 3942; 4873714; {_expectedOutputInfection.TestsConductedTotal}; 7976385"
            };
            string[] yesterdayTestsRegionerContent = new string[]
            {
                "ugenr; Region Hovedstaden; Region Midtjylland; Region Nordjylland; Region Sjælland; Region Syddanmark; Statens Serum Institut; Testcenter Danmark; Total; Kumulativ_total",
                "Uge 5; 1; 0; 0; 0; 0; 2; 0; 3; 3",
                $"Total; 1204000; 581774; 278812; 381410; 652733; 3942; 4873714; {_expectedOutputInfection.TestsConductedTotal - _expectedOutputInfection.TestsConductedToday}; 7976385"
            };
            string[] todayTestPosOverTimeContent = new string[]
            {
                "Date;NewPositive;NotPrevPos;PosPct;PrevPos;Tested;Tested_kumulativ",
                "2020-01-27;       0;       1; 0,0;       0;       1;       1",
                $"I alt;  {_expectedOutputInfection.ConfirmedCasesTotal};7.766.616; 1,2;  65.030;7.831.646;7.831.646"
            };
            string[] yesterdayTestPosOverTimeContent = new string[]
            {
                "Date;NewPositive;NotPrevPos;PosPct;PrevPos;Tested;Tested_kumulativ",
                "2020-01-27;       0;       1; 0,0;       0;       1;       1",
                $"I alt;  {_expectedOutputInfection.ConfirmedCasesTotal - _expectedOutputInfection.ConfirmedCasesToday};7.766.616; 1,2;  65.030;7.831.646;7.831.646"
            };

            _sampleFilesToday.Add(new Tuple<string, string[]>("Deaths_over_time.csv", todayDeathsOverTimeContent));
            _sampleFilesYesterday.Add(new Tuple<string, string[]>("Deaths_over_time.csv", yesterdayDeathsOverTimeContent));
            _sampleFilesToday.Add(new Tuple<string, string[]>("Test_regioner.csv", todayTestsRegionerContent));
            _sampleFilesYesterday.Add(new Tuple<string, string[]>("Test_regioner.csv", yesterdayTestsRegionerContent));
            _sampleFilesToday.Add(new Tuple<string, string[]>("Test_pos_over_time.csv", todayTestPosOverTimeContent));
            _sampleFilesYesterday.Add(new Tuple<string, string[]>("Test_pos_over_time.csv", yesterdayTestPosOverTimeContent));
            _sampleFilesToday.Add(new Tuple<string, string[]>("Newly_admitted_over_time.csv", todayNewlyAdmittedOverTimeContent));
            _sampleFilesYesterday.Add(new Tuple<string, string[]>("Newly_admitted_over_time.csv", yesterdayNewlyAdmittedOverTimeContent));

            // Statistics New

            _sampleFilesStatistics = new List<Tuple<string, string[]>>();

            string[] statisticsContent = new string[]
            {
                // Header
                "Dato;Region;Køn;Bekræftede tilfælde;Døde;Overstået infektion;Indlæggelser;" +
                "Testede personer;Ændring i antal bekræftede tilfælde;Ændring i antal døde;" +
                "Ændring i antal overstået infektion;Ændring i antal indlagte;Ændring i antallet af testede personer;" +
                "Antallet af prøver;Ændring i antallet af prøver;test_AG;test_AG_diff",
                // Data
                "2021 - 03 - 25; Hovedstaden; F; 55023; 616; 52620; 3174; 774135; 196; 2; 110; 4; 821; 4260037; 34875; 431685; 25298",
                "2021 - 03 - 25; Hovedstaden; M; 51472; 670; 48838; 3287; 714278; 202; 0; 99; 6; 928; 3163653; 25134; 409264; 23907",
                "2021 - 03 - 25; Midtjylland; F; 19904; 149; 19208; 841; 514345; 60; 1; 40; 1; 673; 2495468; 21794; 249400; 14289",
                "2021 - 03 - 25; Midtjylland; M; 19065; 193; 18294; 1045; 482053; 47; 0; 39; 2; 815; 1832711; 14481; 254993; 13952",
                "2021 - 03 - 25; NA; ; 0; 0; 0; 0; 21; 0; 0; 0; 0; 0; 33; 0; 0; 0",
                "2021 - 03 - 25; NA; F; 655; 0; 635; 41; 16807; 2; 0; 5; 0; 76; 51777; 521; 22313; 893",
                "2021 - 03 - 25; NA; M; 1409; 2; 1326; 96; 29531; 14; 0; 11; 0; 164; 86101; 1003; 57828; 2248",
                "2021 - 03 - 25; Nordjylland; F; 7578; 69; 7308; 363; 243561; 17; 0; 8; 1; 162; 1294326; 10634; 113454; 5581",
                "2021 - 03 - 25; Nordjylland; M; 7544; 109; 7187; 456; 233603; 20; 0; 16; 1; 205; 953537; 6543; 116243; 5379",
                "2021 - 03 - 25; Sjælland; F; 16247; 172; 15608; 1048; 318192; 42; 0; 26; 1; 509; 1625652; 17068; 165781; 10162",
                "2021 - 03 - 25; Sjælland; M; 14906; 220; 14172; 1241; 295781; 49; 2; 32; 3; 542; 1170251; 11326; 168652; 9929",
                "2021 - 03 - 25; Syddanmark; F; 15910; 96; 14778; 726; 470277; 81; 0; 86; 6; 802; 2359197; 26026; 288328; 32570",
                "2021 - 03 - 25; Syddanmark; M; 16131; 113; 14948; 823; 443710; 84; 0; 77; 3; 856; 1781740; 18009; 275814; 29153"
            };

            _sampleFilesStatistics.Add(new Tuple<string, string[]>("Regionalt_DB/01_noegle_tal.csv", statisticsContent));


            // Vaccination
            _sampleFilesVaccination = new List<Tuple<string, string[]>>();

            string[] todayVaccinationContent = new string[]
            {
                "geografi,Antal første vacc.,Antal faerdigvacc.,Antal borgere,Vacc.dækning faerdigvacc. (%),Vacc.dækning påbegyndt vacc. (%)",
                "DK,362158,180439,5841178,2.1,4.3"
            };
            
            _sampleFilesVaccination.Add(new Tuple<string, string[]>("Vaccine_DB/Vaccinationsdaekning_nationalt.csv", todayVaccinationContent));
        }

        private ZipArchive CreateArchive(List<Tuple<string, string[]>> files)
        {
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var filename = file.Item1;
                    var fileContentLines = file.Item2;
                    var demoFile = archive.CreateEntry(filename);

                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        foreach (var line in fileContentLines)
                        {
                            streamWriter.WriteLine(line);
                        }
                    }
                }
            }

            return new ZipArchive(memoryStream);
        }

        private SSIZipArchivesInfoDto CreateZipInfos()
        {
            return new SSIZipArchivesInfoDto
            {
                DateInfection = _sampleDateInfection,
                TodayArchive = CreateArchive(_sampleFilesToday),
                YesterdayArchive = CreateArchive(_sampleFilesYesterday),
                VaccinationArchive = CreateArchive(_sampleFilesVaccination),
                StatisticsArchive = CreateArchive(_sampleFilesStatistics)
            };
        }

        private static IEnumerable<SSIZipArchivesInfoDto> GetNullDtoArguments()
        {
            yield return null;
            yield return new SSIZipArchivesInfoDto()
            {
                TodayArchive = null,
                YesterdayArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create),
                VaccinationArchive = null
            };
            yield return new SSIZipArchivesInfoDto()
            {
                TodayArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create),
                YesterdayArchive = null,
                VaccinationArchive = null
            };
            yield return new SSIZipArchivesInfoDto()
            {
                TodayArchive = null,
                YesterdayArchive = null,
                VaccinationArchive = null
            };
            yield return new SSIZipArchivesInfoDto()
            {
                TodayArchive = null,
                YesterdayArchive = null,
                VaccinationArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create)
            };
        }

        private SSIZipFileReaderService CreateService()
        {
            return new SSIZipFileReaderService(
                _mockSSIStatisticsRepository.Object,
                _mockSSIStatisticsVaccinationRepository.Object,
                _mockSSIExcelParsingConfig,
                _mockLogger.Object);
        }

        [TestCaseSource(nameof(GetNullDtoArguments))]
        public void HandleSsiZipArchives_NullArgumentPassed_ShouldThrowArgumentException(SSIZipArchivesInfoDto zipInfos)
        {
            // Arrange
            var service = CreateService();

            // Assert
            Assert.Throws(typeof(ArgumentException), () => service.HandleSsiStatisticsZipArchive(zipInfos));
        }

        [Test]
        public void HandleSsiZipArchives_ProperArchivesPassed_ShouldSaveCorrectRecord()
        {
            _mockSSIStatisticsRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Act
            service.HandleSsiStatisticsZipArchive(zipInfos);

            // Assert
            _actualOutputInfection.Should().BeEquivalentTo(_expectedOutputInfection);
        }

        [Test]
        public void HandleSsiZipArchives_RecordAlreadyExistsForADay_RecordShouldBeDeleted()
        {
            var statisticsInjectionToBeDeleted = new SSIStatistics();
            _mockSSIStatisticsRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: statisticsInjectionToBeDeleted);
            _mockSSIStatisticsRepository.Setup(x => x.Delete(statisticsInjectionToBeDeleted)).Verifiable();

            var statisticsVaccinationToBeDeleted = new SSIStatisticsVaccination();
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: statisticsVaccinationToBeDeleted);
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.Delete(statisticsVaccinationToBeDeleted)).Verifiable();

            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Act
            service.HandleSsiStatisticsZipArchive(zipInfos);
            service.HandleSsiVaccinationZipArchive(zipInfos);

            // Assert
            _mockSSIStatisticsRepository.Verify(x => x.Delete(statisticsInjectionToBeDeleted), Times.Once);
            _mockSSIStatisticsVaccinationRepository.Verify(x => x.Delete(statisticsVaccinationToBeDeleted), Times.Once);
        }

        [Test]
        public void HandleSsiZipArchives_ExcelFileIsMissing_ShouldThrowProperException()
        {
            _mockSSIStatisticsRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesToday.RemoveAt(0);

            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesVaccination.RemoveAt(0);

            using var zipInfos = CreateZipInfos();
            var service = CreateService();
            
            var exceptionVaccine = Assert.Throws<SSIZipFileParseException>(() => service.HandleSsiVaccinationZipArchive(zipInfos));
            var match = Regex.IsMatch(exceptionVaccine.Message, "The vaccination percentages excel file .* is missing in zip package");
            Assert.That(match);
        }

        [Test]
        public void HandleSsiZipArchives_ImportantColumnIsMissingInTheExcel_ShouldThrowProperException()
        {
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesVaccination[0] = new Tuple<string, string[]>(_sampleFilesVaccination[0].Item1, new string[]
            {
                "geografi;wrong column name",
                "DK,362158"
            });

            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Assert
            Assert.Throws<SSIZipFileParseException>(() => service.HandleSsiVaccinationZipArchive(zipInfos));
        }

        [Test]
        public void HandleSsiZipArchives_CannotParseValueInUnimportantExcelRow_ShouldFinishCalculations()
        {
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesVaccination[0] = new Tuple<string, string[]>(_sampleFilesVaccination[0].Item1, new string[]
            {
                "geografi,Antal første vacc.,Antal faerdigvacc.,Antal borgere,Vacc.dækning faerdigvacc. (%),Vacc.dækning påbegyndt vacc. (%)",
                "DK,unImportantValue,180439,5841178,2.1,4.3"
            });

            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Assert
            Assert.DoesNotThrow(() => service.HandleSsiVaccinationZipArchive(zipInfos));
        }

        [Test]
        public void HandleSsiVaccinationZipArchives_ValueForVaccineFirstTimeIsNotPositiveDoubleValues_ShouldThrowException()
        {
            // Arrange
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesVaccination[0] = new Tuple<string, string[]>(_sampleFilesVaccination[0].Item1, new string[]
            {
                "geografi,Antal første vacc.,Antal faerdigvacc.,Antal borgere,Vacc.dækning faerdigvacc. (%),Vacc.dækning påbegyndt vacc. (%)",
                "DK,unImportantValue,180439,5841178,2.1,0.0"
            });

            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Assert
            var exceptionVaccine = Assert.Throws<SSIZipFileParseException>(() => service.HandleSsiVaccinationZipArchive(zipInfos));
            Assert.That(exceptionVaccine.Message.Contains("Covid statistics: number for vaccination first time is 0"));
        }

        [Test]
        public void HandleSsiVaccinationZipArchives_ValueForVaccineSecondTimeIsNotPositiveDoubleValues_ShouldThrowException()
        {
            // Arrange
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesVaccination[0] = new Tuple<string, string[]>(_sampleFilesVaccination[0].Item1, new string[]
            {
                "geografi,Antal første vacc.,Antal faerdigvacc.,Antal borgere,Vacc.dækning faerdigvacc. (%),Vacc.dækning påbegyndt vacc. (%)",
                "DK,unImportantValue,180439,5841178,0.0,4.3"
            });

            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Assert
            var exceptionVaccine = Assert.Throws<SSIZipFileParseException>(() => service.HandleSsiVaccinationZipArchive(zipInfos));
            Assert.That(exceptionVaccine.Message.Contains("Covid statistics: number for vaccination second time is 0"));
        }

        [Test]
        public void HandleSsiVaccinationZipArchives_CannotFindColumnForFirstVaccinationPercentages_ShouldThrowException()
        {
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesVaccination[0] = new Tuple<string, string[]>(_sampleFilesVaccination[0].Item1, new string[]
            {
                "geografi,Antal første vacc.,Antal faerdigvacc.,Antal borgere,Vacc.dækning faerdigvacc. (%),WrongColumnName",
                "DK,unImportantValue,180439,5841178,0.0,4.3"
            });
            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Assert
            var exception = Assert.Throws<SSIZipFileParseException>(() => service.HandleSsiVaccinationZipArchive(zipInfos));
            Assert.That(exception.Message.Contains(SSIZipFileReaderService.VaccinationZipFileReadErrorMessage));
        }

        [Test]
        public void HandleSsiVaccinationZipArchives_CannotFindColumnForSecondVaccinationPercentages_ShouldThrowException()
        {
            _mockSSIStatisticsVaccinationRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            _sampleFilesVaccination[0] = new Tuple<string, string[]>(_sampleFilesVaccination[0].Item1, new string[]
            {
                "geografi,Antal første vacc.,Antal faerdigvacc.,Antal borgere,WrongColumnName,Vacc.dækning påbegyndt vacc. (%)",
                "DK,unImportantValue,180439,5841178,0.0,4.3"
            });
            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Assert
            var exception = Assert.Throws<SSIZipFileParseException>(() => service.HandleSsiVaccinationZipArchive(zipInfos));
            Assert.That(exception.Message.Contains(SSIZipFileReaderService.VaccinationZipFileReadErrorMessage));
        }
        [TestCaseSource(nameof(GetNullDtoArguments))]
        public void HandleSsiStatisticsZipArchive_NullArgumentPassed_ShouldThrowArgumentException(SSIZipArchivesInfoDto zipInfos)
        {
            // Arrange
            var service = CreateService();

            // Assert
            Assert.Throws(typeof(ArgumentException), () => service.HandleSsiStatisticsZipArchive(zipInfos));
        }
        [Test]
        public void HandleSsiStatisticsZipArchives_ProperArchivesPassed_ShouldSaveCorrectRecord()
        {
            _mockSSIStatisticsRepository.Setup(x => x.GetEntryByDate(It.IsAny<DateTime>())).Returns(value: null);
            using var zipInfos = CreateZipInfos();
            var service = CreateService();

            // Act
            service.HandleSsiStatisticsZipArchive(zipInfos);

            // Assert
            _actualOutputInfection.Should().BeEquivalentTo(_expectedOutputInfection);
        }
    }
}
