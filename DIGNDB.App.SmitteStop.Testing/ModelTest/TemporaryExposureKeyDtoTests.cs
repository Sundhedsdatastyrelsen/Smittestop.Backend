using DIGNDB.App.SmitteStop.Domain.Dto;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DIGNDB.App.SmitteStop.Testing.ModelTest
{
    [TestFixture]
    public class TemporaryExposureKeyDtoTests
    {
        TemporaryExposureKeyDto _dto;

        [SetUp]
        public void initModel()
        {
            _dto = new TemporaryExposureKeyDto();
        }

        [Test]
        public void TemporaryExposureKeyDto_SetKey_ShouldReturnCorrectValue()
        {
            var expect = Encoding.UTF8.GetBytes("keyData");
            _dto.key = expect;
            Assert.That(_dto.key, Is.EqualTo(expect));
        }

        [Test]
        public void TemporaryExposureKeyDto_SetRollingStart_ShouldReturnCorrectValue()
        {
            var expect = DateTime.UtcNow.Date;
            _dto.rollingStart = expect;
            Assert.That(_dto.rollingStart, Is.EqualTo(expect));
        }

        [Test]
        public void TemporaryExposureKeyDto_SetRollingDurationSpan_ShouldReturnCorrectValue()
        {
            var expect = "2.00:00:00.000";
            _dto.rollingDuration = expect;
            Assert.That(_dto.rollingDurationSpan, Is.EqualTo(TimeSpan.Parse(expect)));
        }

        [Test]
        public void TemporaryExposureKeyDto_SetTransmissionRiskLevel_ShouldReturnCorrectValue()
        {
            var expect = RiskLevel.RISK_LEVEL_LOW_MEDIUM;
            _dto.transmissionRiskLevel = expect;
            Assert.That(_dto.transmissionRiskLevel, Is.EqualTo(expect));
        }

        [TestCase("+00:00", false)]
        [TestCase("+01:00", true)]
        [TestCase("-01:00", false)]
        [TestCase("+12:00", true)]
        [TestCase("-12:00", false)]
        public void TemporaryExposureKeyDto_UTCMidnightJsonConverter_ShouldReturnCorrectValue(string baseUtcOffset, bool isPreviousDayInUTC)
        {
            var baseDate = DateTime.MinValue.AddDays(1);
            var expect = baseDate.Date;

            if (isPreviousDayInUTC)
                expect = baseDate.AddDays(-1);

            var requestBody = MockUploadDiagnosisKeysRequestBody(baseUtcOffset, baseDate);
            var keyBatchDto = JsonSerializer.Deserialize<TemporaryExposureKeyBatchDto>(requestBody);
            keyBatchDto.keys.ForEach(key => Assert.That(key.rollingStart, Is.EqualTo(expect)));
        }

        private string MockUploadDiagnosisKeysRequestBody(string baseUtcOffset, DateTime baseDate)
        {
            JObject jObject = JObject.FromObject(new TemporaryExposureKeyBatchDto() 
            { keys = new List<TemporaryExposureKeyDto>() 
                { new TemporaryExposureKeyDto() 
                    { rollingStart = default } 
                } 
            });
            JToken jToken = jObject.SelectToken("keys[0].rollingStart");
            jToken.Replace(string.Format("{0:yyyy-MM-ddTHH:mm:ss}", baseDate) + baseUtcOffset);

            return jObject.ToString();
        }
    }
}
