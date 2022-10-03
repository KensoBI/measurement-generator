namespace Models
{
    public class Characteristic
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public int FeatureId { get; set; }
        public double Nominal { get; set; }
        public double Usl { get; set; }
        public double Lsl { get; set; }
    }
}