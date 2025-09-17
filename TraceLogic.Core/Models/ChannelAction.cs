namespace TraceLogic.Core.Models
{
    /// <summary>
    /// Represents a detailed action performed by a single pipetting channel.
    /// </summary>
    public class ChannelAction
    {
        public required int ChannelNumber { get; set; }
        public required string LabwareId { get; set; }
        public required string PositionId { get; set; }
        public required double Volume { get; set; }

        public override string ToString()
        {
            return $"Ch: {ChannelNumber}, Labware: {LabwareId}, Pos: {PositionId}, Vol: {Volume}uL";
        }
    }
}
