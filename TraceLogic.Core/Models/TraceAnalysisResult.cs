namespace TraceLogic.Core.Models
{
    /// <summary>
    /// A container for all the data parsed from a .trc file.
    /// This is the final object returned by the parser.
    /// </summary>
    public class TraceAnalysisResult
    {
        public required string FileName { get; set; }
        public List<TraceEntry> AllEntries { get; set; } = new List<TraceEntry>();
        public List<PipettingStep> PipettingSteps { get; set; } = new List<PipettingStep>();

        // NEW: Add a list to hold the high-level liquid transfer events.
        public List<LiquidTransferEvent> LiquidTransfers { get; set; } = new List<LiquidTransferEvent>();

        public List<string> Errors { get; set; } = new List<string>();
    }
}

