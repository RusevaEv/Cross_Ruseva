namespace Cross_Ruseva.Services
{
    public static class DataGeneratorService
    {
        public static List<double> GenerateWeibull(double alpha, double beta, int count)
        {
            var random = new Random();
            var times = new List<double>(count);

            for (int i = 0; i < count; i++)
            {
                // Избегаем 0 и 1 для корректного логарифма
                double u = Math.Clamp(random.NextDouble(), 1e-6, 1.0 - 1e-6);
                double t = alpha * Math.Pow(-Math.Log(u), 1.0 / beta);
                times.Add(Math.Round(t, 2));
            }

            times.Sort();
            return times;
        }
    }
}
