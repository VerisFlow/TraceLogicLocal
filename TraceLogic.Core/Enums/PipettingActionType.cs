namespace TraceLogic.Core.Enums
{
    /// <summary>
    /// Defines the specific type of a pipetting action.
    /// </summary>
    public enum PipettingActionType
    {
        Unknown,
        Initialize,
        Aspirate,
        Dispense,
        PickupTip,
        EjectTip
    }
}
