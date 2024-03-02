namespace MeasurementGenerator;

public static class GaussianRandom
{
    private static Random _random = new Random();
    private static bool _hasDeviate;
    private static double _storedDeviate;

    public static double NextGaussian(this Random r, double mean = 0, double stdDev = 1)
    {
        if (_hasDeviate)
        {
            _hasDeviate = false;
            return mean + stdDev * _storedDeviate;
        }

        double u, v, s;
        do
        {
            u = 2 * r.NextDouble() - 1;
            v = 2 * r.NextDouble() - 1;
            s = u * u + v * v;
        } while (s is >= 1 or 0);

        s = Math.Sqrt(-2 * Math.Log(s) / s);
        _storedDeviate = v * s;
        _hasDeviate = true;
        return mean + stdDev * u * s;
    }
}