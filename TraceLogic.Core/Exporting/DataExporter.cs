using System.Text;
using TraceLogic.Core.Models;

namespace TraceLogic.Core.Exporting
{
    /// <summary>
    /// Handles exporting data to various file formats.
    /// This version is simplified to only support CSV export.
    /// </summary>
    public static class DataExporter
    {
        /// <summary>
        /// Exports a list of liquid transfer events to a CSV file.
        /// </summary>
        public static void ExportToCsv(IEnumerable<LiquidTransferEvent> data, List<DataGridColumnInfo> columns, string filePath)
        {
            var sb = new StringBuilder();

            // Add header row without any quoting
            sb.AppendLine(string.Join(",", columns.Select(c => c.Header)));

            // Add data rows
            foreach (var transfer in data)
            {
                var line = string.Join(",", columns.Select(c =>
                {
                    var value = GetPropertyValue(transfer, c.PropertyName);
                    // Safely convert the potentially null value to a string for the CSV.
                    var stringValue = value?.ToString() ?? string.Empty;
                    // Return the raw string value without escaping or quoting
                    return stringValue;
                }));
                sb.AppendLine(line);
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Gets a property's value from an object using reflection.
        /// </summary>
        private static object? GetPropertyValue(object obj, string propertyName)
        {
            // Ensure propertyName is not null or empty before trying to get the property
            if (string.IsNullOrEmpty(propertyName) || obj == null)
            {
                return null;
            }
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }
    }

    /// <summary>
    /// A simple DTO to carry column information from UI to the exporter.
    /// </summary>
    public class DataGridColumnInfo
    {
        public required string Header { get; set; }
        public required string PropertyName { get; set; }
    }
}

