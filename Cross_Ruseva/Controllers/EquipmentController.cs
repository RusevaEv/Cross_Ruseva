using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cross_Ruseva.Models;
using System;
using System.Linq;

namespace Cross_Ruseva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
       // CRUD операции (создание, чтение, удаление)

        // GET: Получить все датасеты
        [HttpGet("datasets")]
        public IActionResult GetDatasets()
        {
            return Ok(DataStore.Datasets);
        }

        // POST: Добавить новый датасет (Create)
        [Authorize(Roles = "Admin")]
        [HttpPost("datasets")]
        public IActionResult CreateDataset([FromBody] EquipmentDataset dataset)
        {
            dataset.Id = Guid.NewGuid(); // Генерируем новый ID
            dataset.CleanData();         // Применяем бизнес-логику (убираем отрицательные числа)
            DataStore.Datasets.Add(dataset);
            return Ok(dataset);
        }

        // POST: Добавить метрики надежности для конкретного датасета
        [HttpPost("metrics")]
        public IActionResult AddMetrics([FromBody] ReliabilityMetrics metrics)
        {
            metrics.Id = Guid.NewGuid();
            DataStore.Metrics.Add(metrics);
            return Ok(metrics);
        }

        // DELETE: Удалить датасет по ID
        [HttpDelete("datasets/{id}")]
        public IActionResult DeleteDataset(Guid id)
        {
            var dataset = DataStore.Datasets.FirstOrDefault(d => d.Id == id);
            if (dataset == null) return NotFound("Датасет не найден");

            DataStore.Datasets.Remove(dataset);
            // Удаляем и связанные метрики
            DataStore.Metrics.RemoveAll(m => m.DatasetId == id);

            return Ok("Удалено");
        }

        // 2. LINQ-ЗАПРОСЫ НА ОБЪЕДИНЕНИЕ ДАННЫХ

        // ЗАПРОС 1: Сводный отчет (Объединяем Dataset и Metrics через LINQ Select и Where)
        [Authorize]
        [HttpGet("reports/summary")]
        public IActionResult GetSummaryReport()
        {
            // Берем метрики, находим их датасет и собираем анонимный объект
            var report = DataStore.Metrics.Select(m => new
            {
                MetricId = m.Id,
                Phase = m.GetLifecyclePhase(),       // Вызов бизнес-логики из модели
                Alpha = m.Alpha,
                Beta = m.Beta,
                // Ищем связанный датасет по ID
                DatasetName = DataStore.Datasets.FirstOrDefault(d => d.Id == m.DatasetId)?.Name ?? "Неизвестно",
                AverageTime = DataStore.Datasets.FirstOrDefault(d => d.Id == m.DatasetId)?.CalculateMeanTimeToFailure() ?? 0
            }).ToList();

            return Ok(report);
        }

        // ЗАПРОС 2: Поиск проблемного оборудования (Фильтрация Where по объединенным данным)
        [HttpGet("reports/critical")]
        public IActionResult GetCriticalEquipment()
        {
            // Фильтруем: выводим только те связки, где стадия деградации (Beta > 2)
            var criticalData = DataStore.Metrics
                .Where(m => m.Beta > 2.0)
                .Select(m => new
                {
                    DatasetName = DataStore.Datasets.FirstOrDefault(d => d.Id == m.DatasetId)?.Name,
                    Warning = "Критический износ!",
                    BetaValue = m.Beta
                }).ToList();

            return Ok(criticalData);
        }

        [HttpPost("upload-csv")]
        [Authorize(Roles = "Admin")]
        public IActionResult UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран или пуст");

            var times = Services.CsvParser.ParseFailures(file);
            var dataset = new EquipmentDataset
            {
                Name = file.FileName,
                OperatingTimes = times
            };

            DataStore.Datasets.Add(dataset);
            return Ok(dataset);
        }

        [HttpPost("datasets/{id}/estimate-weibull")]
        [Authorize(Roles = "Admin")]
        public IActionResult EstimateWeibull(Guid id)
        {
            var dataset = DataStore.Datasets.FirstOrDefault(d => d.Id == id);
            if (dataset == null || !dataset.OperatingTimes.Any())
                return NotFound("Датасет не найден или пуст");

            var metrics = Services.FitEngine.EstimateWeibull(dataset);

            // Удаляем старые метрики для этого датасета, если они были, и сохраняем новые
            DataStore.Metrics.RemoveAll(m => m.DatasetId == id);
            DataStore.Metrics.Add(metrics);

            return Ok(new
            {
                Message = "Расчет выполнен успешно",
                Phase = metrics.GetLifecyclePhase(),
                Result = metrics
            });
        }
    }
}