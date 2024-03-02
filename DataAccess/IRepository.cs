using Kenso.Domain;
using Models;

namespace DataAccess
{
    public interface IRepository
    {
        Task<CharacteristicRecord[]> GetCharacteristics(long[] partIds);
        Task Save(IList<MeasurementRecord> measurements);
    }
}