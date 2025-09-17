using TraceLogic.Core.Enums;

namespace TraceLogic.Core.Models
{
    /// <summary>
    /// Represents a higher-level, aggregated pipetting operation (e.g., a full aspirate or dispense).
    /// </summary>
    public class PipettingStep
    {
        public PipettingActionType ActionType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public List<ChannelAction> ChannelActions { get; set; } = new List<ChannelAction>();
        public int StartLineNumber { get; set; }
    }
}
