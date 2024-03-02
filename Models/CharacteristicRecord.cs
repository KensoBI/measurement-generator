namespace Models
{
    public class CharacteristicRecord
    {
        public long Id { get; set; }
        public long PartId { get; set; }
        public decimal Nominal { get; set; }
        public double Usl { get; set; }
        public double Lsl { get; set; }
    }
}