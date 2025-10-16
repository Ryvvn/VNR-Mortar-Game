using System.Collections.Generic;
using System.IO;

namespace MortarGame.Utility
{
    public static class SimpleCSV
    {
        // Very light CSV splitter assuming no quoted commas in fields.
        public static List<string[]> Parse(string filePath)
        {
            var rows = new List<string[]>();
            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = line.Split(',');
                    rows.Add(cols);
                }
            }
            return rows;
        }
    }
}