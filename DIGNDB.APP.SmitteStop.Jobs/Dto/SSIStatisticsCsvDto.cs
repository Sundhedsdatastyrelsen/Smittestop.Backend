using System.Collections.Generic;

namespace DIGNDB.APP.SmitteStop.Jobs.Dto
{
    public class SSIStatisticsCsvDto
    {
        public List<DeathsOverTimeDto> DeathsOverTime { get; set; }
        public List<NewlyAdmittedOverTimeDto> NewlyAdmittedOverTime { get; set; }
        public List<TestPosOverTimeDto> TestPosOverTime { get; set; }
        public List<TestedExcelDto> TestedExcel { get; set; }
        public VaccinationPercentagesDto VaccinationPercentages { get; set; }
        public List<StatisticsDto> Statistics { get; set; }
    }
}