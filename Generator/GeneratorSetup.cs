using DataAccess.Kenso;
using DataAccess.QDAS;
using Models;
using Microsoft.Extensions.Configuration;

namespace Generator
{
    public class GeneratorSetup
    {
        private readonly IConfiguration _configuration;

        public GeneratorSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GeneratorOptions? GetOptions(int? startMonth, int? endMonth, int? numOfMeasurements, int? schema)
        {
            var options = new GeneratorOptions();
            options.StartDate = GetStartDate(startMonth);
            options.EndDate = GetEndDate(options.StartDate, endMonth);
            options.NumberOfMeasurements = GetNumberOfMeasurements(numOfMeasurements);
            options.StandardDev = GetStdDev();

            var repo = GetRepoForSchema(schema);

            if (repo == null)
            {
                Console.WriteLine("Connection string not set or schema not supported.");
                return null;
            }

            options.Repository = repo;
            return options;
        }

        public static DateTime GetStartDate(int? startMonth)
        {
            var startDate = DateTime.UtcNow.AddMonths(-1);

            if (startMonth.HasValue)
            {
                startDate = DateTime.Now.AddMonths(startMonth.Value);
            }

            Console.WriteLine($"Start date set to: {startDate.Date.ToShortDateString()}");
            return startDate;
        }

        public static DateTime GetEndDate(DateTime startDate, int? endMonth)
        {
            var defaultEndDate = DateTime.UtcNow.AddDays(7);
            var endDate = defaultEndDate;

            if (endMonth.HasValue)
            {
                endDate = DateTime.Now.AddMonths(endMonth.Value);

                if (startDate.Date >= endDate.Date)
                {
                    Console.WriteLine("Start date must start before end date! ");
                    endDate = defaultEndDate;
                }
            }

            Console.WriteLine($"End date set to: {endDate.Date.ToShortDateString()}");
            return endDate;
        }

        public static int GetNumberOfMeasurements(int? numOfMeasurements)
        {
            var numberOfMeasurements = 1000;

            if (numOfMeasurements is > 0)
            {
                numberOfMeasurements = numOfMeasurements.Value;
            }

            Console.WriteLine($"Measurements per characteristic set to: {numberOfMeasurements}");
            return numberOfMeasurements;
        }

        public double GetStdDev()
        {
            var stdDev = 3.0;
            var stdDevSection = _configuration.GetSection("StandardDeviation");

            if (stdDevSection != null)
            {
                _ = double.TryParse(stdDevSection.Value, out stdDev);
            }

            Console.WriteLine($"Standard deviation set to {stdDev}");
            return stdDev;
        }

        public IRepository? GetRepoForSchema(int? schema)
        {
            string connectionString;

            if (schema is not > 0) return null;

            switch (schema)
            {
                case 1:
                    Console.WriteLine("Database set to Postgres, schema: Kenso");
                    connectionString = _configuration.GetConnectionString("postgres-kenso");
                    return new KensoPgRepository(connectionString);
                case 2:
                    Console.WriteLine("Database set to Postgres, schema: QDAS");
                    connectionString = _configuration.GetConnectionString("postgres-qdas");
                    return new QdasPgRepository(connectionString);

            }

            return null;
        }
    }
}