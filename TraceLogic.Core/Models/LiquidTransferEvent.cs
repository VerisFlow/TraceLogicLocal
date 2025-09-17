namespace TraceLogic.Core.Models
{
    /// <summary>
    /// Represents a single, complete liquid transfer from a source to a target.
    /// This is a high-level model generated from aggregating multiple PipettingSteps.
    /// Properties are nullable to allow for state tracking during parsing.
    /// </summary>
    public class LiquidTransferEvent
    {
        public DateTime Timestamp { get; set; }
        public int ChannelId { get; set; }
        public string? SourceLabware { get; set; }
        public string? SourcePositionId { get; set; }
        public string? TargetLabware { get; set; }
        public string? TargetPositionId { get; set; }
        public double Volume { get; set; }
        public string? TipLabwareId { get; set; }
        public int TipPositionId { get; set; }
    }
}

