using System.Globalization;

namespace Cross_Ruseva.Services
{
    public static class CsvParser
    {
        public static (List<double> Times, List<string> Errors) ParseFailures(IFormFile file)
        {
            var times = new List<double>();
            var errors = new List<string>();

            using var reader = new StreamReader(file.OpenReadStream());
            int lineNumber = 0;

            while (!reader.EndOfStream)
            {
                lineNumber++;
                var rawLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                // Поддержка точек, запятых и точек с запятой
                var parts = rawLine.Split(new[] { ';', '	' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var clean = part.Replace(',', '.').Trim();
                    if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    {
                        if (value <= 0)
                            errors.Add($"Строка {lineNumber}: значение '{value}' должно быть больше 0.");
                        else
                            times.Add(value);
                    }
                    else if (lineNumber > 1) // Игнорируем возможный текстовый заголовок в 1-й строке
                    {
                        errors.Add($"Строка {lineNumber}: '{part}' не является числом.");
                    }
                }
            }

            return (times, errors);
        }
    }
}
