using Cross_Ruseva.Models;

namespace Cross_Ruseva.Services
{
    public static class FitEngine
    {
        public static ReliabilityMetrics EstimateWeibull(EquipmentDataset dataset)
        {
            var times = dataset.OperatingTimes.OrderBy(t => t).ToList();
            int n = times.Count;

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;

            for (int i = 0; i < n; i++)
            {
                // Медианные ранги Бернарда
                double f = (i + 1 - 0.3) / (n + 0.4);
                double x = Math.Log(times[i]);
                double y = Math.Log(-Math.Log(1.0 - f));

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
                sumY2 += y * y;
            }

            double denomX = n * sumX2 - sumX * sumX;
            double denomY = n * sumY2 - sumY * sumY;

            double beta = denomX != 0 ? (n * sumXY - sumX * sumY) / denomX : 1.0;
            double intercept = (sumY - beta * sumX) / n;
            double alpha = Math.Exp(-intercept / beta);

            double rSquared = 0;
            if (denomX > 0 && denomY > 0)
            {
                double r = (n * sumXY - sumX * sumY) / Math.Sqrt(denomX * denomY);
                rSquared = Math.Round(r * r, 4);
            }

            return new ReliabilityMetrics
            {
                DatasetId = dataset.Id,
                DistributionType = "Weibull",
                Alpha = Math.Round(alpha, 2),
                Beta = Math.Round(beta, 2),
                RSquared = rSquared
            };
        }
    }
}
