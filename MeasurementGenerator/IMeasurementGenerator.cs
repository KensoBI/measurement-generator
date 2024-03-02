using Kenso.Domain;
using Models;

namespace MeasurementGenerator
{
    public interface IMeasurementGenerator
    {
        Task<MeasurementSimple[]> GetMeasurementsForOneHour(DateTime startDateTime);
        Task<MeasurementRecord[]> GetMeasurementsForDate(DateTime measurementDate);
    }
}
