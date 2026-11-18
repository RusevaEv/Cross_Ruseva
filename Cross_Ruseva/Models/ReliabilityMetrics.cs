namespace Cross_Ruseva.Models
{
    public class ReliabilityMetrics
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public string DistributionType { get; set; } = "Weibull";
        public double Alpha { get; set; } // Параметр масштаба (характеристическая наработка)
        public double Beta { get; set; }  // Параметр формы (стадия износа)
        public double RSquared { get; set; } // Коэффициент детерминации (точность подгонки)

        // Бизнес-логика: вероятность безотказной работы P(t)
        public double CalculateReliability(double time)
        {
            if (time < 0 || Alpha <= 0 || Beta <= 0) return 0;
            return Math.Round(Math.Exp(-Math.Pow(time / Alpha, Beta)), 4);
        }

        // Бизнес-логика: определение стадии жизненного цикла по параметру Beta
        public string GetLifecyclePhase()
        {
            if (Beta < 0.9) return "Период приработки (ранние отказы)";
            if (Beta <= 1.2) return "Период нормальной эксплуатации (случайные отказы)";
            if (Beta <= 2.0) return "Начальная стадия старения";
            return "Период интенсивного износа и деградации";
        }
    }
}
