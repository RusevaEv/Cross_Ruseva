using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Cross_Ruseva.Services
{
    public static class CsvParser
    {
        public static List<double> ParseFailures(IFormFile file)
        {
            var times = new List<double>();
            using var reader = new StreamReader(file.OpenReadStream());

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (double.TryParse(line?.Replace('.', ','), out double time) && time > 0)
                {
                    times.Add(time);
                }
            }
            return times;
        }
    }
}