using Kenso.Data.Kafka;
using Kenso.Domain;
using Confluent.Kafka;
using Kenso.Data.Repository;
using Microsoft.Extensions.Options;
using Models;

namespace MeasurementGeneratorConsumer
{
    public class MeasurementMessageHandler : IMessageHandler<string, MeasurementSimple>
    {
        private readonly ILogger<MeasurementMessageHandler> _logger;
        private readonly IMeasurementRepository _repository;
        private readonly IAssetRepository _assetRepository;
        private readonly KafkaOptions _kafkaOptions;
        private string _assetName = string.Empty;
        private Asset? _asset;

        public MeasurementMessageHandler(ILogger<MeasurementMessageHandler> logger, IMeasurementRepository repository, IAssetRepository assetRepository, IOptions<KafkaOptions> kafkaOptions)
        {
            _logger = logger;
            _repository = repository;
            _assetRepository = assetRepository;
            _kafkaOptions = kafkaOptions.Value;
        }

        public async Task Handle(Message<string, MeasurementSimple> cr)
        {
            try
            {
                _logger.LogInformation($"Processing {cr.Key}");
                var measurement = new Measurement();
                measurement.Characteristic = new Characteristic { Id = cr.Value.CharacteristicId };
                measurement.Value = cr.Value.Value;
                measurement.DateTime = cr.Value.DateTime;
                measurement.Asset = Asset;
                measurement.Serial = cr.Value.Serial;
                measurement.CreatedBy = _kafkaOptions.Topic;
                await _repository.Insert(measurement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving measurement. {ex.Message}");
            }
        }

        public Asset Asset
        {
            get
            {
                if (_asset != null) return _asset;

                var topicParts = _kafkaOptions.Topic.Split(".");
                _asset = new Asset
                {
                    Name = _kafkaOptions.Topic,
                    Location = _kafkaOptions.Topic
                };

                if (topicParts.Length >= 5)
                {
                    _asset = new Asset
                    {
                        Location = string.Join(".", topicParts.Take(4)),
                        Name = topicParts[^2]
                    };
                }

                _asset.Id = _assetRepository.Upsert(_asset, "measurement-generator").Result;
                return _asset;
            }
        }
    }
}
