using System;
using System.Collections.Generic;
using System.Linq;

namespace Cross_Ruseva.Models
{
    public class EquipmentDataset
    {
        // свойства
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<double> OperatingTimes { get; set; } = new List<double>();

        // методы
        public void CleanData()
        {
            if (OperatingTimes != null)
            {
                OperatingTimes = OperatingTimes.Where(t => t > 0).ToList();
            }
        }

        public double CalculateMeanTimeToFailure()
        {
            if (OperatingTimes == null || !OperatingTimes.Any())
                return 0;

            return OperatingTimes.Average();
        }
    }
}
