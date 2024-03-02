namespace Models
{
    public class MeasurementSimple
    {
        public long CharacteristicId { get; set; }
        public Decimal Value { get; set; }
        public DateTime DateTime { get; set; }
        public string? Serial { get; set; }
    }
}