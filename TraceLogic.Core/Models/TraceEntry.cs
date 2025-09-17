using TraceLogic.Core.Enums;

namespace TraceLogic.Core.Models
{
    /// <summary>
    /// Represents a single parsed line from the .trc log file.
    /// This is the lowest-level data object.
    /// </summary>
    public class TraceEntry
    {
        public int LineNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Source { get; set; }
        public required string Command { get; set; }
        public EntryStatus Status { get; set; }
        public required string Details { get; set; }
        public required string RawLine { get; set; }
    }
}
