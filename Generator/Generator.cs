using Models;

namespace Generator
{
    public class Generator
    {
        private readonly GeneratorOptions _options;
        private readonly double _stepSize;
        private readonly Random _rand;

        public Generator(GeneratorOptions options)
        {
            _options = options;
            _stepSize = Math.Abs(_options.StandardDev) / 10;
            _rand = new Random();
        }

        public async Task Start()
        {
            var characteristics = await _options.Repository.GetCharacteristics(Array.Empty<int>());

            await Generate(characteristics.ToArray());
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

        public async Task Generate(Characteristic[] characteristics)
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
        }

        public List<Measurement> GetMeasurementsForDate(Characteristic[] characteristics, DateTime measurementDate, int measPerDay)
        {
            var measurements = new List<Measurement>();
            var ticks = TimeSpan.TicksPerDay / measPerDay;

            foreach (var characteristic in characteristics)
            {
                if (characteristic.Nominal == 0)
                {
                    continue;
                }

                var measPerDayCounter = 0;
                var currentValue = characteristic.Nominal - _rand.NextDouble();
                var measDay = measurementDate.Date;
                var dateDateTime = measDay;
                while (measPerDay > measPerDayCounter)
                {
                    measPerDayCounter++;
                    dateDateTime = dateDateTime.AddTicks(ticks);

                    var charMeas = new Measurement();
                    charMeas.Time = DateTime.SpecifyKind(dateDateTime, DateTimeKind.Utc);
                    charMeas.PartId = characteristic.PartId;
                    charMeas.CharacteristicId = characteristic.Id;
                    charMeas.Value = CalculateNextValue(currentValue, characteristic.Usl, characteristic.Lsl);
                    currentValue = charMeas.Value;
                    measurements.Add(charMeas);
                }
            }

            return measurements;
        }
    }
}
