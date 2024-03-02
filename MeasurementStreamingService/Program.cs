using DataAccess.Kenso;
using Kenso.Data.Kafka;
using Kenso.Data.Repository;
using MeasurementGenerator;
using MeasurementStreamingService;
using Models;
using IRepository = DataAccess.IRepository;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<KafkaOptions>().BindConfiguration("Kafka");
        services.AddOptions<DatabaseOptions>().BindConfiguration("Database");
        services.AddOptions<GeneratorOptions>().BindConfiguration("Generator");
        services.AddSingleton<IRepository, KensoPgRepository>();
        services.AddSingleton<IMeasurementGenerator, Generator>();
        services.AddSingleton<ClientHandle>();
        services.AddSingleton<DependentProducer<string, MeasurementSimple>>();
        //services.Configure<List<MeasurementJob>>(hostContext.Configuration.GetSection("MeasurementJobs"));
        services.AddHostedService<MeasurementStreamingWorker>();
    })
    .Build();

var producer = host.Services.GetRequiredService<DependentProducer<string, Models.MeasurementSimple>>();
await producer.BuildProducer();

host.Run();

