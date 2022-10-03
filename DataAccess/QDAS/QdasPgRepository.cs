using Models;

namespace DataAccess.QDAS
{
    public class QdasPgRepository : IRepository 
    {
        public Task<IList<Characteristic>> GetCharacteristics(int[] partIds)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetMaxMeasurementId()
        {
            throw new NotImplementedException();
        }

        public Task Save(IList<Measurement> measurements)
        {
            throw new NotImplementedException();
        }
    }
}
