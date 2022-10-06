namespace Models
{
    public class Measurement
    {
        public int Id { get; set; }
        public int CharacteristicId { get; set; }
        public int PartId { get; set; }
        public double Value { get; set; }
        public DateTime Time { get; set; }
    }
}