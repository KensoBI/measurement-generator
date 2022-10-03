// See https://aka.ms/new-console-template for more information

using DataAccess.Kenso;
using MeasurementGenerator;
using Microsoft.Extensions.Configuration;
using Models;

IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var startDate = DateTime.UtcNow.AddMonths(-1);
var endDate = DateTime.UtcNow.AddDays(7);
var numberOfMeasurements = 1000;
var partIds = new List<int>();
var schema = 1;
IRepository? repository = null;

if (true)//(args.Length > 0)
{
    //todo
    foreach (var arg in args)
    {
        Console.WriteLine($"Argument={arg}");
    }
}
else
{
    Console.WriteLine(
        "Enter first date of the measurement. You can also type a number of months to add to the current date, i.e. -3");
    var startDateStr = Console.ReadLine();
    if (!string.IsNullOrEmpty(startDateStr))
    {
        if (int.TryParse(startDateStr, out int monthBack))
        {
            startDate = DateTime.Now.AddMonths(monthBack);
        }
        else
        {
            _ = DateTime.TryParse(startDateStr.Trim(), out startDate);
        }

    }

    Console.WriteLine($"Start date set to: {startDate.Date.ToShortDateString()}");

    var endDateIsValid = false;

    while (!endDateIsValid)
    {
        Console.WriteLine(
            "Enter last date of the measurement. You can also type a number of months to add to the current date, i.e. 2.");
        var endDateStr = Console.ReadLine();

        if (!string.IsNullOrEmpty(endDateStr))
        {
            if (int.TryParse(endDateStr, out int monthBack))
            {
                endDate = DateTime.Now.AddMonths(monthBack);
            }
            else
            {
                _ = DateTime.TryParse(endDateStr.Trim(), out endDate);
            }

            if (startDate.Date >= endDate.Date)
            {
                Console.WriteLine("Start date must start before end date! ");
            }
            else
            {
                endDateIsValid = true;
            }

        }
    }
    Console.WriteLine($"End date set to: {endDate.Date.ToShortDateString()}");
    Console.WriteLine("Enter total number of measurements to generate for each characteristic.");
    var numberOfMeasurementsStr = Console.ReadLine();

    if (int.TryParse(numberOfMeasurementsStr, out int numberOfMeasurementsProvided))
    {
        if (numberOfMeasurementsProvided > 0)
        {
            numberOfMeasurements = numberOfMeasurementsProvided;
        }
    }
    Console.WriteLine($"Measurements per characteristic set to: {numberOfMeasurements}");

    Console.WriteLine("Select database type and schema: \n1 - Postgres, schema: Kenso\n2 - Postgres, schema: QDAS");

    var schemaStr = Console.ReadLine();
    _ = int.TryParse(schemaStr, out schema);
}

switch (schema)
{
    case 1:
        Console.WriteLine("Database set to Postgres, schema: Kenso");
        var connectionString = config.GetConnectionString("postgres-kenso");
        repository = new KensoPgRepository(connectionString);
        break;
    case 2:
        Console.WriteLine("Database set to Postgres, schema: QDAS");
        break;
}

//todo read config

if (repository != null)
{
    var gen = new Generator(repository, 3, numberOfMeasurements, startDate, endDate);
    await gen.Start();
}


Console.WriteLine("Done.");
