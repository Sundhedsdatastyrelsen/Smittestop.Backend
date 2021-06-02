using DIGNDB.App.SmitteStop.Domain.Db;
using System.Collections.Generic;

namespace DIGNDB.App.SmitteStop.DAL.Seeders
{
    public class CountrySeeder
    {
        public const long DenmarkCountryId = 7L;
        public const long EnglandCountryId = 28L;

        public IEnumerable<Country> GetSeedData()
        {
            return new[]
            {
                new Country {Id = 1, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "AT"},
                new Country {Id = 2, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "BE"},
                new Country {Id = 3, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "BG"},
                new Country {Id = 4, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "HR"},
                new Country {Id = 5, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "CY"},
                new Country {Id = 6, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "CZ"},
                new Country {Id = 8, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "EE"},
                new Country {Id = 9, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "FI"},
                new Country {Id = 10, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "FR"},
                new Country {Id = 11, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "DE"},
                new Country {Id = 12, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "GR"},
                new Country {Id = 13, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "HU"},
                new Country {Id = 14, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "IE"},
                new Country {Id = 15, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "IT"},
                new Country {Id = 16, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "LV"},
                new Country {Id = 17, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "LT"},
                new Country {Id = 18, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "LU"},
                new Country {Id = 19, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "MT"},
                new Country {Id = 20, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "NL"},
                new Country {Id = 21, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "PL"},
                new Country {Id = 22, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "PT"},
                new Country {Id = 23, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "RO"},
                new Country {Id = 24, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "SK"},
                new Country {Id = 25, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "SI"},
                new Country {Id = 26, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "ES"},
                new Country {Id = 27, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = false, Code = "SE"},
                new Country {Id = 29, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "NO"},
                new Country {Id = 30, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "IS"},
                new Country {Id = 31, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "LI"},
                new Country {Id = 32, PullingFromGatewayEnabled = true, VisitedCountriesEnabled = true, Code = "CH"},

                new Country {Id = DenmarkCountryId, PullingFromGatewayEnabled = false, VisitedCountriesEnabled = false, Code = "DK"},
                new Country {Id = EnglandCountryId, PullingFromGatewayEnabled = false, VisitedCountriesEnabled = false, Code = "EN"}
            };
        }
    }
}
