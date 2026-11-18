using Cross_Ruseva.Services;

namespace Cross_Ruseva.Models
{
    // Временное хранилище в памяти (готово к замене на EF Core DbContext)
    public static class DataStore
    {
        private static int _nextDatasetId = 1;
        private static int _nextMetricsId = 1;

        public static List<EquipmentDataset> Datasets { get; } = new();
        public static List<ReliabilityMetrics> Metrics { get; } = new();

        public static int GetNextDatasetId() => _nextDatasetId++;
        public static int GetNextMetricsId() => _nextMetricsId++;

        static DataStore()
        {
            // Начальные тестовые данные (Seed), чтобы отчеты работали сразу после запуска
            var pumpDataset = new EquipmentDataset
            {
                Id = GetNextDatasetId(),
                Name = "Насос НПВ-100 (износ)",
                OperatingTimes = new List<double> { 420, 480, 510, 535, 560, 590, 610, 645 }
            };
            Datasets.Add(pumpDataset);

            var pumpMetrics = FitEngine.EstimateWeibull(pumpDataset);
            pumpMetrics.Id = GetNextMetricsId();
            Metrics.Add(pumpMetrics);
        }
    }
}
