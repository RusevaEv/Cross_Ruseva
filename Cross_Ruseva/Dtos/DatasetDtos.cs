using System.ComponentModel.DataAnnotations;

namespace Cross_Ruseva.Dtos
{
    public class CreateDatasetDto : IValidatableObject
    {
        [Required(ErrorMessage = "Название выборки обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Список наработок обязателен")]
        [MinLength(3, ErrorMessage = "Для расчета надежности требуется минимум 3 значения наработки")]
        public List<double> OperatingTimes { get; set; } = new();

        // Жесткая входная валидация вместо тихой очистки
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OperatingTimes.Any(t => t <= 0 || double.IsNaN(t) || double.IsInfinity(t)))
            {
                yield return new ValidationResult(
                    "Все значения наработки на отказ должны быть строго больше нуля.",
                    new[] { nameof(OperatingTimes) });
            }
        }
    }

    public class GenerateWeibullDto
    {
        [Required(ErrorMessage = "Укажите название синтетической выборки")]
        public string Name { get; set; } = "Синтетическая выборка";

        [Range(1.0, 1_000_000.0, ErrorMessage = "Параметр Alpha должен быть > 0")]
        public double Alpha { get; set; } = 500;

        [Range(0.1, 20.0, ErrorMessage = "Параметр Beta должен быть от 0.1 до 20")]
        public double Beta { get; set; } = 2.5;

        [Range(5, 1000, ErrorMessage = "Количество точек должно быть от 5 до 1000")]
        public int Count { get; set; } = 20;
    }
}
