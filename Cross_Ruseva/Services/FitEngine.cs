using System;
using System.Linq;
using Cross_Ruseva.Models;

namespace Cross_Ruseva.Services
{
    public static class FitEngine
    {
        public static ReliabilityMetrics EstimateWeibull(EquipmentDataset dataset)
        {
            var times = dataset.OperatingTimes.OrderBy(t => t).ToList();
            int n = times.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                // Эмпирическая вероятность (медианные ранги Бернарда)
                double f = (i + 1 - 0.3) / (n + 0.4);

                // Двойное логарифмирование для перехода к линейному уравнению
                double x = Math.Log(times[i]);
                double y = Math.Log(-Math.Log(1.0 - f));

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            // Формулы линейной регрессии
            double beta = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - beta * sumX) / n;
            double alpha = Math.Exp(-intercept / beta);

            return new ReliabilityMetrics
            {
                DatasetId = dataset.Id,
                Alpha = Math.Round(alpha, 2),
                Beta = Math.Round(beta, 2)
            };
        }
    }
}