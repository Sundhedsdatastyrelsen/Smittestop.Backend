namespace DIGNDB.APP.SmitteStop.Jobs.Dto
{
    public class StatisticsDto
    {
        public int ConfirmedCases { get; set; }
        public int Died { get; set; }
        public int ChangedConfirmedCases { get; set; }
        public int ChangedDied { get; set; }
        public int NumberSamples { get; set; }
        public int NumberSamplesAG { get; set; }
        public int ChangedNumberSamplesPcr { get; set; }
        public int ChangedNumberSamplesAntigen { get; set; }
    }
}