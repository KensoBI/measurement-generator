using Kenso.Data.Kafka;
using MeasurementGeneratorConsumer;
using Kenso.Data.Repository.Postgres;
using Kenso.Data.Repository;
using Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<KafkaOptions>().BindConfiguration("Kafka");
        services.AddOptions<DatabaseOptions>().BindConfiguration("Database");
        services.AddSingleton<Consumer<string, MeasurementSimple>>();
        services.AddSingleton<IMessageHandler<string, MeasurementSimple>, MeasurementMessageHandler>();
        services.AddSingleton<IMeasurementRepository, MeasurementRepository>();
        services.AddSingleton<IAssetRepository, AssetRepository>();
        services.AddHostedService<Worker>();

    })
    .Build();

host.Run();