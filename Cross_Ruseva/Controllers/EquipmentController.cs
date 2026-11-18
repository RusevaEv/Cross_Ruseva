using Cross_Ruseva.Dtos;
using Cross_Ruseva.Models;
using Cross_Ruseva.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cross_Ruseva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        // READ: Все выборки (доступно всем)
        [HttpGet("datasets")]
        public IActionResult GetAllDatasets()
        {
            return Ok(DataStore.Datasets);
        }

        // READ: Одна выборка по int ID
        [HttpGet("datasets/{id:int}")]
        public IActionResult GetDatasetById(int id)
        {
            var dataset = DataStore.Datasets.FirstOrDefault(d => d.Id == id);
            if (dataset == null)
                return NotFound($"Датасет с ID = {id} не найден.");

            return Ok(dataset);
        }

        // CREATE: Создание выборки с жесткой валидацией DTO
        [HttpPost("datasets")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateDataset([FromBody] CreateDatasetDto dto)
        {
            var dataset = new EquipmentDataset
            {
                Id = DataStore.GetNextDatasetId(),
                Name = dto.Name,
                OperatingTimes = dto.OperatingTimes
            };

            DataStore.Datasets.Add(dataset);
            return CreatedAtAction(nameof(GetDatasetById), new { id = dataset.Id }, dataset);
        }

        // UPDATE: Обновление выборки по int ID
        [HttpPut("datasets/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateDataset(int id, [FromBody] CreateDatasetDto dto)
        {
            var dataset = DataStore.Datasets.FirstOrDefault(d => d.Id == id);
            if (dataset == null)
                return NotFound($"Датасет с ID = {id} не найден.");

            dataset.Name = dto.Name;
            dataset.OperatingTimes = dto.OperatingTimes;
            return Ok(dataset);
        }

        // DELETE: Каскадное удаление выборки и связанных метрик
        [HttpDelete("datasets/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteDataset(int id)
        {
            var dataset = DataStore.Datasets.FirstOrDefault(d => d.Id == id);
            if (dataset == null)
                return NotFound($"Датасет с ID = {id} не найден.");

            DataStore.Datasets.Remove(dataset);
            DataStore.Metrics.RemoveAll(m => m.DatasetId == id);
            return NoContent();
        }

        // CREATE (CSV): Загрузка выборки из CSV-файла
        [HttpPost("datasets/upload-csv")]
        [Authorize(Roles = "Admin")]
        public IActionResult UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран или пуст.");

            var (times, errors) = CsvParser.ParseFailures(file);
            if (errors.Any())
                return BadRequest(new { Message = "Ошибки в CSV-файле", Errors = errors });

            if (times.Count < 3)
                return BadRequest("В файле должно быть минимум 3 положительных значения наработки.");

            var dataset = new EquipmentDataset
            {
                Id = DataStore.GetNextDatasetId(),
                Name = Path.GetFileNameWithoutExtension(file.FileName),
                OperatingTimes = times
            };

            DataStore.Datasets.Add(dataset);
            return CreatedAtAction(nameof(GetDatasetById), new { id = dataset.Id }, dataset);
        }

        // CREATE (Synthetic): Генерация синтетической выборки Вейбулла
        [HttpPost("datasets/generate-synthetic")]
        [Authorize(Roles = "Admin")]
        public IActionResult GenerateSynthetic([FromBody] GenerateWeibullDto dto)
        {
            var times = DataGeneratorService.GenerateWeibull(dto.Alpha, dto.Beta, dto.Count);
            var dataset = new EquipmentDataset
            {
                Id = DataStore.GetNextDatasetId(),
                Name = dto.Name,
                IsSynthetic = true,
                OperatingTimes = times
            };

            DataStore.Datasets.Add(dataset);
            return CreatedAtAction(nameof(GetDatasetById), new { id = dataset.Id }, dataset);
        }

        // MATH: Расчет параметров Вейбулла (Alpha, Beta, R^2)
        [HttpPost("datasets/{id:int}/estimate-weibull")]
        [Authorize(Roles = "Admin")]
        public IActionResult EstimateWeibull(int id)
        {
            var dataset = DataStore.Datasets.FirstOrDefault(d => d.Id == id);
            if (dataset == null)
                return NotFound($"Датасет с ID = {id} не найден.");

            var metrics = FitEngine.EstimateWeibull(dataset);
            metrics.Id = DataStore.GetNextMetricsId();

            DataStore.Metrics.RemoveAll(m => m.DatasetId == id);
            DataStore.Metrics.Add(metrics);

            return Ok(new
            {
                Message = "Расчет Вейбулла выполнен успешно",
                Phase = metrics.GetLifecyclePhase(),
                MTTF = dataset.CalculateMeanTimeToFailure(),
                Metrics = metrics
            });
        }

        // LINQ (Select + объединение): Сводный отчет по всему оборудованию
        [HttpGet("reports/summary")]
        [Authorize]
        public IActionResult GetSummaryReport()
        {
            var report = DataStore.Metrics
                .Select(m =>
                {
                    var ds = DataStore.Datasets.FirstOrDefault(d => d.Id == m.DatasetId);
                    return new
                    {
                        DatasetId = m.DatasetId,
                        EquipmentName = ds?.Name ?? "Неизвестно",
                        MTTF = ds?.CalculateMeanTimeToFailure() ?? 0,
                        m.Alpha,
                        m.Beta,
                        m.RSquared,
                        LifecyclePhase = m.GetLifecyclePhase(),
                        ReliabilityAt500h = m.CalculateReliability(500)
                    };
                })
                .ToList();

            return Ok(report);
        }

        // LINQ (Where + Select): Критическое оборудование (Beta > 2.0)
        [HttpGet("reports/critical")]
        [Authorize]
        public IActionResult GetCriticalEquipment()
        {
            var critical = DataStore.Metrics
                .Where(m => m.Beta > 2.0)
                .Select(m =>
                {
                    var ds = DataStore.Datasets.FirstOrDefault(d => d.Id == m.DatasetId);
                    return new
                    {
                        DatasetId = m.DatasetId,
                        EquipmentName = ds?.Name ?? "Неизвестно",
                        m.Beta,
                        Warning = "Критический износ! Требуется планово-предупредительный ремонт."
                    };
                })
                .ToList();

            return Ok(critical);
        }
    }
}
