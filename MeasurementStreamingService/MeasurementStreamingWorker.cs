using System.Collections.Concurrent;
using Confluent.Kafka;
using Kenso.Data.Kafka;
using Kenso.Domain;
using MeasurementGenerator;
using Models;

namespace MeasurementStreamingService;

public class MeasurementStreamingWorker : BackgroundService
{
    private readonly ILogger<MeasurementStreamingWorker> _logger;
    private readonly IMeasurementGenerator _measurementGenerator;
    private readonly DependentProducer<string, MeasurementSimple> _kafkaProducer;
    private readonly ConcurrentQueue<MeasurementSimple> _measurements = new();

    public MeasurementStreamingWorker(ILogger<MeasurementStreamingWorker> logger, IMeasurementGenerator measurementGenerator, DependentProducer<string, MeasurementSimple> kafkaProducer)
    {
        _logger = logger;
        _measurementGenerator = measurementGenerator;
        _kafkaProducer = kafkaProducer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_measurements.IsEmpty)
            {
                try
                {
                    var measurements = await _measurementGenerator.GetMeasurementsForOneHour(DateTime.UtcNow);
                    foreach (var measurementRecord in measurements.OrderBy(p=> p.DateTime))
                    {
                        _measurements.Enqueue(measurementRecord);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    await Task.Delay(1000, stoppingToken);
                }
            }

            if (_measurements.TryDequeue(out var rec))
            {
                while (rec.DateTime > DateTime.UtcNow)
                {
                    await Task.Delay(500, stoppingToken);
                }

                try
                {
   
                    PublishMeasurement(rec);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            }
        }
    }

    public void PublishMeasurement(MeasurementSimple measurement)
    {
        var key = $"characteristicId:{measurement.CharacteristicId}";
        _kafkaProducer.Produce(new Message<string, MeasurementSimple>
            {
                Key = key,
                Value = measurement
            },
            (deliveryReport) =>
            {
                if (deliveryReport.Error.Code != ErrorCode.NoError)
                {
                    _logger.LogError($"Failed to deliver message: {deliveryReport.Error.Reason}");

                }
                else
                {
                    _logger.LogInformation($"Produced event: key = {key}");
                }
            });
    }
}
