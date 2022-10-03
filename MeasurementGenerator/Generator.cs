using Models;

namespace MeasurementGenerator
{
    public class Generator
    {
        private readonly IRepository _repository;
        private readonly double _standardDev;
        private readonly int _numberOfMeasurements;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly double _stepSize;
        private readonly Random _rand;

        public Generator(IRepository repository, double standardDev, int numberOfMeasurements, DateTime startDate, DateTime endDate)
        {
            _repository = repository;
            _standardDev = standardDev;
            _numberOfMeasurements = numberOfMeasurements;
            _startDate = startDate;
            _endDate = endDate;
            _stepSize = Math.Abs(standardDev) / 10;
            _rand = new Random();
        }

        public async Task Start()
        {
            var characteristics = await _repository.GetCharacteristics(Array.Empty<int>());

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

            var chance = _standardDev / 2 - distance / 50;
            var randomValue = _standardDev * _rand.NextDouble();

            // if the random value is smaller than the chance we continue in the
            // current direction if not we change the direction.
            return randomValue < chance ? continueDirection : changeDirection;
        }

        public async Task Generate(Characteristic[] characteristics)
        {
            var range = _endDate - _startDate;
            int startCounter = 0;
            int measPerDay = _numberOfMeasurements / range.Days;
            var timeSpent = new System.Diagnostics.Stopwatch();

            while (range.Days > startCounter)
            {
                var measDate = _startDate.AddDays(startCounter++);
                var measurements = GetMeasurementsForDate(characteristics, measDate, measPerDay);

                timeSpent.Restart();
                await _repository.Save(measurements);
                timeSpent.Stop();

                Console.WriteLine(startCounter + "/" + range.Days + ". Saved " + measurements.Count + " in " + timeSpent.ElapsedMilliseconds / 1000 + "s.");
            }
        }

        public List<Measurement> GetMeasurementsForDate(Characteristic[] characteristics, DateTime measurementDate, int measPerDay)
        {
            var measurements = new List<Measurement>();

            foreach (var characteristic in characteristics)
            {
                var measPerDayCounter = 0;
                
                var currentValue = characteristic.Nominal - _rand.NextDouble();
                while (measPerDay > measPerDayCounter)
                {
                    measPerDayCounter++;
                    measurementDate = measurementDate.AddHours(23.0 / measPerDay);

                    var charMeas = new Measurement();
                    charMeas.CreateTimestamp = DateTime.SpecifyKind(measurementDate, DateTimeKind.Utc);
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
