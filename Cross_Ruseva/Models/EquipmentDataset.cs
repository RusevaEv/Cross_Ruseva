namespace Cross_Ruseva.Models
{
    public class EquipmentDataset
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSynthetic { get; set; } = false;
        public List<double> OperatingTimes { get; set; } = new();

        // Бизнес-логика: средняя наработка на отказ (MTTF)
        public double CalculateMeanTimeToFailure()
        {
            if (OperatingTimes.Count == 0) return 0;
            return Math.Round(OperatingTimes.Average(), 2);
        }
    }
}
