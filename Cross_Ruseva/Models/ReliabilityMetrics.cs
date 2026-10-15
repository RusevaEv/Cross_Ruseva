using System;

namespace Cross_Ruseva.Models
{
    public class ReliabilityMetrics
    {
        // свойства
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DatasetId { get; set; }

        public double Alpha { get; set; }
        public double Beta { get; set; }

        // методы
        // 1. Расчет вероятности безотказной работы R(t) для заданного времени t
        public double CalculateReliability(double time)
        {
            if (time <= 0) return 1.0;
            return Math.Exp(-Math.Pow(time / Alpha, Beta));
        }

        // 2. Анализ фазы жизненного цикла оборудования на основе параметра формы
        public string GetLifecyclePhase()
        {
            if (Beta < 1)
                return "Период приработки (наличие скрытых дефектов)";
            if (Beta >= 1 && Beta <= 1.5)
                return "Нормальная эксплуатация (случайные отказы)";

            return "Период деградации и износа (требуется обслуживание)";
        }

    }
}
