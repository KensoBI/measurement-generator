using DataAccess;
using Kenso.Domain;
using Microsoft.Extensions.Options;
using Models;

namespace MeasurementGenerator
{
    public class Generator : IMeasurementGenerator
    {
        private readonly GeneratorOptions _options;
        private readonly IRepository _repository;
        private readonly double _stepSize;
        private readonly Random _rand;

        public Generator(IOptions<GeneratorOptions> options, IRepository repository)
        {
            _options = options.Value;
            _repository = repository;
            _stepSize = Math.Abs(_options.StandardDev) / 10;
            _rand = new Random();
        }

        public double CalculateNextValue(double currentValue, double usl, double lsl)
        {
            var range = usl - lsl;
            //var rangeFactor = range * factor;

            // first calculate how much the value will be changed
            var valueChange = _rand.NextDouble() * _stepSize;
            // second decide if the value is increased or decreased
            var factor = DecideUpDown(valueChange, range);
            // apply valueChange and factor to value and return
            return currentValue + valueChange * factor;
        }

        public double DecideUpDown(double value, double nominal)
        {
            int continueDirection, changeDirection;
            double distance;

            if (value > nominal)
            {
                distance = value - nominal;
                continueDirection = 1;
                changeDirection = -1;

            }
            else
            {
                distance = nominal - value;
                continueDirection = -1;
                changeDirection = 1;
            }

            // the chance is calculated by taking half of the standardDeviation
            // and subtracting the distance divided by 50. This is done because
            // chance with a distance of zero would mean a 50/50 chance for the
            // randomValue to be higher or lower.
            // The division by 50 was found by empiric testing different values

            var chance = _options.StandardDev / 2 - distance / 50;
            var randomValue = _options.StandardDev * _rand.NextDouble();

            // if the random value is smaller than the chance we continue in the
            // current direction if not we change the direction.
            return randomValue < chance ? continueDirection : changeDirection;
        }

        /*public async Task Generate(Characteristic[] characteristics)
        {
            var range = _options.EndDate - _options.StartDate;
            int startCounter = 0;
            int measPerDay = _options.NumberOfMeasurements / range.Days;
            var timeSpent = new System.Diagnostics.Stopwatch();

            while (range.Days > startCounter)
            {
                var measDate = _options.StartDate.AddDays(startCounter++);
                var measurements = GetMeasurementsForDate(characteristics, measDate, measPerDay);

                timeSpent.Restart();
                await _options.Repository.Save(measurements);
                timeSpent.Stop();

                Console.WriteLine(startCounter + "/" + range.Days + ". Saved " + measurements.Count + " in " + timeSpent.ElapsedMilliseconds / 1000 + "s.");
            }
        }*/

        public async Task<MeasurementRecord[]> GetMeasurementsForDate(DateTime measurementDate)
        {
            var measurementRecords = new List<MeasurementRecord>();
            var ticks = TimeSpan.TicksPerDay / _options.NumberOfMeasurementsPerDay;
            var characteristics = await GetCharacteristics();
     
            foreach (var characteristic in characteristics)
            {
                if (characteristic.Nominal == 0)
                {
                    continue;
                }

                var measPerDayCounter = 0;
                var currentValue = (double)characteristic.Nominal - _rand.NextDouble();
                var measDay = measurementDate.Date;
                var dateDateTime = measDay;
                while (_options.NumberOfMeasurementsPerDay > measPerDayCounter)
                {
                    measPerDayCounter++;
                    dateDateTime = dateDateTime.AddTicks(ticks);

                    var measurementRecord = new MeasurementRecord
                    {
                        CharacteristicId = characteristic.Id,
                        PartId = characteristic.PartId,
                        Nominal = characteristic.Nominal,
                        Usl = (decimal?)characteristic.Usl,
                        Lsl = (decimal?)characteristic.Lsl,
                        MeasurementDate = DateTime.SpecifyKind(dateDateTime, DateTimeKind.Utc),
                        MeasurementValue = (decimal?)CalculateNextValue(currentValue, characteristic.Usl!, characteristic.Lsl!)
                    };
                    
                    measurementRecords.Add(measurementRecord);
                    currentValue = (double)measurementRecord.MeasurementValue;
                }
            }

            return measurementRecords.ToArray();
        }

        public async Task<CharacteristicRecord[]> GetCharacteristics()
        {
            var parts = new List<long>();
            if (_options.PartId.HasValue)
            {
                parts.Add(_options.PartId.Value);
            }
            return await _repository.GetCharacteristics(parts.ToArray());
        }

        public async Task<MeasurementSimple[]> GetMeasurementsForOneHour(DateTime startDateTime)
        {
            var characteristics = await GetCharacteristics();
            var measurementRecords = new List<MeasurementSimple>();
            var measPerHour = _options.NumberOfMeasurementsPerDay / 24;
            var radnomSec = new Random();

            foreach (var partIdGroup in characteristics.GroupBy(c => c.PartId))
            {
                var partSerials = new List<string>();
                for (int i = 0; i < measPerHour; i++)
                {
                    partSerials.Add(Guid.NewGuid().ToString()[..8]);
                }

                foreach (var characteristic in partIdGroup)
                {
                    if (characteristic.Nominal == 0)
                    {
                        continue;
                    }
                    var ticksPerMeasurement = TimeSpan.TicksPerHour / measPerHour;
                    var currentValue = (double)characteristic.Nominal - _rand.NextDouble();
                    var currentTime = startDateTime;

                    for (int i = 0; i < measPerHour; i++)
                    {
                        currentTime = currentTime.AddTicks(ticksPerMeasurement).AddSeconds(radnomSec.Next(0, 30));
                        var measurement = new MeasurementSimple
                        {
                            CharacteristicId = characteristic.Id,
                            Serial = partSerials[i], // Assign the same serial number to all characteristics of the part
                            DateTime = DateTime.SpecifyKind(currentTime, DateTimeKind.Utc),
                            Value = (decimal)CalculateNextValue(currentValue, characteristic.Usl!, characteristic.Lsl!)
                        };

                        measurementRecords.Add(measurement);
                        currentValue = (double)measurement.Value;
                    }
                }
            }

            return measurementRecords.ToArray();
        }

    }
}
