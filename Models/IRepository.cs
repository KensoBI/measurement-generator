namespace Models
{
    public interface IRepository
    {
        Task<IList<Characteristic>> GetCharacteristics(int[] partIds);
        Task Save(IList<Measurement> measurements);
    }
}