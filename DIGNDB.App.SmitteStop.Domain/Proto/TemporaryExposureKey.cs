﻿using System.Text;
using Google.Protobuf;

namespace DIGNDB.App.SmitteStop.Domain.Proto
{
    public partial class TemporaryExposureKeyExport
    {
        public const string HeaderContents = "EK Export v1    ";

        public static readonly byte[] Header = Encoding.UTF8.GetBytes(HeaderContents);
    }

    public partial class TemporaryExposureKey
    {
        public TemporaryExposureKey(byte[] keyData, int rollingStart, int rollingDuration, int transmissionRisk, Types.ReportType reportType, int daysSinceOnsetOfSymptoms)
        {
            KeyData = ByteString.CopyFrom(keyData);
            RollingStartIntervalNumber = rollingStart;
            RollingPeriod = rollingDuration;
            TransmissionRiskLevel = transmissionRisk;
            ReportType = reportType;
            DaysSinceOnsetOfSymptoms = daysSinceOnsetOfSymptoms;
        }
    }

    public partial class SignatureInfo
    {
        public string PrivateKey { get; set; }
    }
}