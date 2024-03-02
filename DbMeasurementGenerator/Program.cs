using System.CommandLine;
using MeasurementGenerator;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var startMonthOption = new Option<int?>(
    name: "--startMonth",
    description: "Enter number of months to add to the current date to construct a start date, i.e. -3");
//var startDateOption = new Option<DateTime?>(
//    name: "--startDate",
//    description: "Enter start date.");
var endMonthOption = new Option<int?>(
    name: "--endMonth",
    description: "Enter number of months to add to the current date to construct an end date, i.e. -3");
//var endDateOption = new Option<DateTime?>(
//    name: "--endDate",
//    description: "Enter end date.");
var numberOfMeasurementsOption = new Option<int?>(
    name: "--measurements",
    description: "Enter total number of measurements to generate for each characteristic.");

var schemaOption = new Option<int?>(
    name: "--schema",
    description: "Select database type and schema: \n1 - Postgres, schema: Kenso\n2 - Postgres, schema: QDAS");

var rootCommand = new RootCommand("Measurement generator for KensoBI and QDAS schemas.");
rootCommand.AddOption(startMonthOption);
rootCommand.AddOption(endMonthOption);
rootCommand.AddOption(numberOfMeasurementsOption);
rootCommand.AddOption(schemaOption);

rootCommand.SetHandler(async (startMonth, endMonth, numOfMeasurements, schema) =>
    {
        var genSetup = new GeneratorSetup(config);
        var options = genSetup.GetOptions(startMonth, endMonth, numOfMeasurements, schema);
        if (options == null)
        {
            Console.WriteLine("Supplied options are not valid.");
            return;
        }
        //todo
        //var gen = new Generator(options);
        //await gen.Start();
    },
    startMonthOption, endMonthOption, numberOfMeasurementsOption, schemaOption);

await rootCommand.InvokeAsync(args);

Console.WriteLine("Done.");