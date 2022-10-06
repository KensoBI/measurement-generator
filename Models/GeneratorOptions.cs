namespace Models
{
    public class GeneratorOptions
    {
        public IRepository Repository { get; set; }
        public double StandardDev { get; set; }
        public int NumberOfMeasurements{ get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Truncate { get; set; }
    }
}
