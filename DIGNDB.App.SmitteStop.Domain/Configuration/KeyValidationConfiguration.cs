using System.Collections.Generic;

namespace DIGNDB.App.SmitteStop.Domain.Configuration
{
    public class KeyValidationConfiguration
    {
        public int OutdatedKeysDayOffset { get; set; }
        public List<string> Regions { get; set; }
        public PackageNameConfig PackageNames { get; set; }
    }
}
