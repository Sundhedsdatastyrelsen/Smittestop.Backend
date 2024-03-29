﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DIGNDB.App.SmitteStop.Domain.Db
{
    public partial class Country
    {
        public long Id { get; set; }
        public bool PullingFromGatewayEnabled { get; set; }
        public bool VisitedCountriesEnabled { get; set; }
        [StringLength(2)]
        public string Code { get; set; }

        public virtual ICollection<Translation> EntityTranslations { get; set; }
        public virtual ICollection<TemporaryExposureKeyCountry> TemporaryExposureKeyCountries { get; set; }
    }
}
