using System.Collections.Generic;

namespace Cross_Ruseva.Models
{
    public static class DataStore
    {
        // Имитация таблицы датасетов
        public static List<EquipmentDataset> Datasets { get; set; } = new();

        // Имитация таблицы метрик надежности
        public static List<ReliabilityMetrics> Metrics { get; set; } = new();
    }
}