namespace Models
{
    public class GeneratorOptions
    {
        public double StandardDev { get; set; }
        public int NumberOfMeasurementsPerDay { get; set; }
        public long? PartId { get; set; }
        public string AssetKey { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
