using FederationGatewayApi.Models;
using FederationGatewayApi.Services;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DIGNDB.App.SmitteStop.Testing.ServiceTest.Gateway
{
    [TestFixture]
    public class DaysSinceOnsetOfSymptomsDecoderTests
    {
        readonly DaysSinceOnsetOfSymptomsDecoder _decoder = new DaysSinceOnsetOfSymptomsDecoder();

        private const int MinSymptomaticPreciseDateRange = -14;
        private const int MaxSymptomaticPreciseDateRange = 21;

        private static readonly IEnumerable<int> SymptomaticPreciseDateRange =
            Enumerable.Range(MinSymptomaticPreciseDateRange,
                MaxSymptomaticPreciseDateRange - MinSymptomaticPreciseDateRange + 1);

        [Test]
        public void TestDecode([ValueSource(nameof(SymptomaticPreciseDateRange))] int dsosInGatewayFormat)
        {
            var results = _decoder.Decode(dsosInGatewayFormat);

            var expected = new DaysSinceOnsetOfSymptomsResults(true)
            {
                Offset = 0,
                SymptomStatus = SymptomStatus.Symptomatic,
                IntervalDuration = 1,
                DateType = DateType.PreciseDate,
                DaysSinceOnsetOfSymptoms = dsosInGatewayFormat
            };

            results.Should().BeEquivalentTo(expected);
        }

        private const int MinSymptomaticUnknownDateRange = 1986;
        private const int MaxSymptomaticUnknownDateRange = 2000;

        private static readonly IEnumerable<int> SymptomaticUnknownDateRange =
            Enumerable.Range(MinSymptomaticUnknownDateRange,
                MaxSymptomaticUnknownDateRange - MinSymptomaticUnknownDateRange + 1);

        [Test]
        public void TestDecodeSymptomaticUnknownDateRange([ValueSource(nameof(SymptomaticUnknownDateRange))] int dsosInGatewayFormat)
        {
            var results = _decoder.Decode(dsosInGatewayFormat);

            const int expectedOffset = 2000;
            var expected = new DaysSinceOnsetOfSymptomsResults(true)
            {
                Offset = expectedOffset,
                SymptomStatus = SymptomStatus.Symptomatic,
                IntervalDuration = null,
                DateType = DateType.Unknown,
                DaysSinceOnsetOfSymptoms = dsosInGatewayFormat - expectedOffset
            };

            results.Should().BeEquivalentTo(expected);
        }

        private const int MinAsymptomaticUnknownDateRange = 2986;
        private const int MaxAsymptomaticUnknownDateRange = 3000;

        private static readonly IEnumerable<int> AsymptomaticUnknownDateRange =
            Enumerable.Range(MinAsymptomaticUnknownDateRange,
                MaxAsymptomaticUnknownDateRange - MinAsymptomaticUnknownDateRange + 1);

        [Test]
        public void TestDecodeAsymptomaticUnknownDateRange([ValueSource(nameof(AsymptomaticUnknownDateRange))] int dsosInGatewayFormat)
        {
            var results = _decoder.Decode(dsosInGatewayFormat);

            const int expectedOffset = 3000;
            var expected = new DaysSinceOnsetOfSymptomsResults(true)
            {
                Offset = expectedOffset,
                SymptomStatus = SymptomStatus.Asymptomatic,
                IntervalDuration = null,
                DateType = DateType.Unknown,
                DaysSinceOnsetOfSymptoms = dsosInGatewayFormat - expectedOffset
            };

            results.Should().BeEquivalentTo(expected);
        }

        private const int MinUnknownSymptomStatusUnknownDateRange = 3986;
        private const int MaxUnknownSymptomStatusUnknownDateRange = 4000;

        private static readonly IEnumerable<int> UnknownSymptomStatusUnknownDateRange =
            Enumerable.Range(MinUnknownSymptomStatusUnknownDateRange,
                MaxUnknownSymptomStatusUnknownDateRange - MinUnknownSymptomStatusUnknownDateRange + 1);

        [Test]
        public void TestDecodeUnknownSymptomStatusUnknownDateRange([ValueSource(nameof(UnknownSymptomStatusUnknownDateRange))] int dsosInGatewayFormat)
        {
            var results = _decoder.Decode(dsosInGatewayFormat);

            const int expectedOffset = 4000;
            var expected = new DaysSinceOnsetOfSymptomsResults(true)
            {
                Offset = expectedOffset,
                SymptomStatus = SymptomStatus.Unknown,
                IntervalDuration = null,
                DateType = DateType.Unknown,
                DaysSinceOnsetOfSymptoms = dsosInGatewayFormat - expectedOffset
            };

            results.Should().BeEquivalentTo(expected);
        }

        private static readonly IEnumerable<int> InvalidRanges = Enumerable.Empty<int>()
            .Concat(Enumerable.Range(22, 85 - 22 + 1))
            .Concat(Enumerable.Range(122, 185 - 122 + 1))
            .Concat(Enumerable.Range(222, 285 - 222 + 1))
            .Concat(Enumerable.Range(322, 385 - 322 + 1))
            .Concat(Enumerable.Range(422, 485 - 422 + 1))
            .Concat(Enumerable.Range(522, 585 - 522 + 1))
            .Concat(Enumerable.Range(622, 685 - 622 + 1))
            .Concat(Enumerable.Range(722, 785 - 722 + 1))
            .Concat(Enumerable.Range(822, 885 - 822 + 1))
            .Concat(Enumerable.Range(922, 985 - 922 + 1))
            .Concat(Enumerable.Range(1022, 1085 - 1022 + 1))
            .Concat(Enumerable.Range(1122, 1185 - 1122 + 1))
            .Concat(Enumerable.Range(1222, 1285 - 1222 + 1))
            .Concat(Enumerable.Range(1322, 1385 - 1322 + 1))
            .Concat(Enumerable.Range(1422, 1485 - 1422 + 1))
            .Concat(Enumerable.Range(1522, 1585 - 1522 + 1))
            .Concat(Enumerable.Range(1622, 1685 - 1622 + 1))
            .Concat(Enumerable.Range(1722, 1785 - 1722 + 1))
            .Concat(Enumerable.Range(1822, 1885 - 1822 + 1))
            .Concat(Enumerable.Range(1922, 1985 - 1922 + 1))
            .Concat(Enumerable.Range(2001, 2985 - 2001 + 1))
            .Concat(Enumerable.Range(3001, 3985 - 3001 + 1))
            .Concat(Enumerable.Range(4001, 4050 - 4001 + 1));

        [Test]
        public void TestDecodeInvalidRanges([ValueSource(nameof(InvalidRanges))] int dsosInGatewayFormat)
        {
            var results = _decoder.Decode(dsosInGatewayFormat);

            results.IsValid.Should().BeFalse();
        }

        private static readonly IEnumerable<int> SymptomaticSymptomStatusRangeDateTypeRange = Enumerable.Empty<int>()
            .Concat(Enumerable.Range(86, 121 - 86 + 1))
            .Concat(Enumerable.Range(186, 221 - 186 + 1))
            .Concat(Enumerable.Range(286, 321 - 286 + 1))
            .Concat(Enumerable.Range(386, 421 - 386 + 1))
            .Concat(Enumerable.Range(486, 521 - 486 + 1))
            .Concat(Enumerable.Range(586, 621 - 586 + 1))
            .Concat(Enumerable.Range(686, 721 - 686 + 1))
            .Concat(Enumerable.Range(786, 821 - 786 + 1))
            .Concat(Enumerable.Range(886, 921 - 886 + 1))
            .Concat(Enumerable.Range(986, 1021 - 986 + 1))
            .Concat(Enumerable.Range(1086, 1121 - 1086 + 1))
            .Concat(Enumerable.Range(1186, 1221 - 1186 + 1))
            .Concat(Enumerable.Range(1286, 1321 - 1286 + 1))
            .Concat(Enumerable.Range(1386, 1421 - 1386 + 1))
            .Concat(Enumerable.Range(1486, 1521 - 1486 + 1))
            .Concat(Enumerable.Range(1586, 1621 - 1586 + 1))
            .Concat(Enumerable.Range(1686, 1721 - 1686 + 1))
            .Concat(Enumerable.Range(1786, 1821 - 1786 + 1))
            .Concat(Enumerable.Range(1886, 1921 - 1886 + 1));

        [Test]
        public void TestDecodeSymptomaticSymptomStatusRangeDateType([ValueSource(nameof(SymptomaticSymptomStatusRangeDateTypeRange))] int dsosInGatewayFormat)
        {
            var results = _decoder.Decode(dsosInGatewayFormat);

            var expected = new DaysSinceOnsetOfSymptomsResults(true)
            {
                Offset = (int)Math.Round(dsosInGatewayFormat / 100d) * 100,
                SymptomStatus = SymptomStatus.Symptomatic,
                IntervalDuration = (int)Math.Round(dsosInGatewayFormat / 100d),
                DateType = DateType.Range,
                DaysSinceOnsetOfSymptoms = dsosInGatewayFormat - (int)Math.Round(dsosInGatewayFormat / 100d) * 100,
            };

            results.Should().BeEquivalentTo(expected);
        }
    }
}