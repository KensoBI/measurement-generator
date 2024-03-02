using Kenso.Data.Kafka;
using Kenso.Domain;
using Models;

namespace MeasurementGeneratorConsumer
{
    public class Worker : BackgroundService
    {
        private readonly Consumer<string, MeasurementSimple> _measurementConsumer;

        public Worker(Consumer<string, MeasurementSimple> measurementConsumer)
        {
            _measurementConsumer = measurementConsumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _measurementConsumer.StartConsumerLoop(stoppingToken);
        }
    }
}
