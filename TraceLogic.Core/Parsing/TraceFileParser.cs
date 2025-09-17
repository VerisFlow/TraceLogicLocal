using System.Globalization;
using System.Text.RegularExpressions;
using TraceLogic.Core.Enums;
using TraceLogic.Core.Models;

namespace TraceLogic.Core.Parsing
{
    /// <summary>
    /// Main class responsible for parsing Hamilton Venus .trc files.
    /// </summary>
    public class TraceFileParser
    {
        // Regex to parse a standard trace line.
        private static readonly Regex LineRegex = new Regex(
            @"^(?<timestamp>[\d\- :]+)> (?<source>.+?) : (?<command>.+?) - (?<status>\w+); ?(?<details>.*)$",
            RegexOptions.Compiled);

        // Regex to parse detailed channel actions from a "complete" line for Aspirate/Dispense.
        private static readonly Regex PipettingDetailsRegex = new Regex(
            @"channel (?<channel>\d+): (?<labware>[^,]+), (?<position>[^,]+), (?<volume>[\d\.]+) uL",
            RegexOptions.Compiled);

        // Regex to parse detailed channel actions for Tip Pick Up/Eject, which do not have a volume.
        private static readonly Regex TipActionDetailsRegex = new Regex(
            @"channel (?<channel>\d+): (?<labware>[^,]+), (?<position>[^,>]+)",
            RegexOptions.Compiled);

        /// <summary>
        /// Parses the entire .trc file from the given path.
        /// </summary>
        /// <param name="filePath">The full path to the .trc file.</param>
        /// <returns>A TraceAnalysisResult object containing all parsed data.</returns>
        public TraceAnalysisResult Parse(string filePath)
        {
            var result = new TraceAnalysisResult { FileName = Path.GetFileName(filePath) };

            try
            {
                result.AllEntries = ParseLines(filePath);
                result.PipettingSteps = AggregatePipettingSteps(result.AllEntries);
                result.LiquidTransfers = CreateLiquidTransferEvents(result.PipettingSteps);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An unexpected error occurred during parsing: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Step 1: Reads the file and parses each line into a TraceEntry object.
        /// </summary>
        private List<TraceEntry> ParseLines(string filePath)
        {
            var entries = new List<TraceEntry>();
            int lineNumber = 1;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var match = LineRegex.Match(line);

                        if (match.Success)
                        {
                            entries.Add(new TraceEntry
                            {
                                LineNumber = lineNumber,
                                Timestamp = DateTime.ParseExact(match.Groups["timestamp"].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                                Source = match.Groups["source"].Value.Trim(),
                                Command = match.Groups["command"].Value.Trim(),
                                Status = Enum.TryParse<EntryStatus>(match.Groups["status"].Value, true, out var status) ? status : EntryStatus.Unknown,
                                Details = match.Groups["details"].Value.Trim(),
                                RawLine = line
                            });
                        }
                        lineNumber++;
                    }
                }
            }
            return entries;
        }

        /// <summary>
        /// Step 2: Aggregates individual TraceEntry objects into logical PipettingSteps.
        /// </summary>
        private List<PipettingStep> AggregatePipettingSteps(List<TraceEntry> entries)
        {
            var pipettingSteps = new List<PipettingStep>();
            for (int i = 0; i < entries.Count - 1; i++) // Iterate up to the second to last entry
            {
                var currentEntry = entries[i];

                // Identify the start of a pipetting command
                if (currentEntry.Status == EntryStatus.Start && IsPipettingCommand(currentEntry.Command))
                {
                    // Find the matching "complete" entry which contains the detailed data
                    var completeEntry = entries.Skip(i + 1).FirstOrDefault(e => e.Command == currentEntry.Command && e.Status == EntryStatus.Complete);

                    if (completeEntry != null)
                    {
                        var actionType = GetPipettingActionType(currentEntry.Command);
                        var step = new PipettingStep
                        {
                            ActionType = actionType,
                            StartTime = currentEntry.Timestamp,
                            EndTime = completeEntry.Timestamp,
                            StartLineNumber = currentEntry.LineNumber,
                            ChannelActions = ParseChannelDetails(completeEntry.Details, actionType)
                        };
                        pipettingSteps.Add(step);

                        // Advance the index to avoid re-processing these lines
                        i = completeEntry.LineNumber - 1;
                    }
                }
            }
            return pipettingSteps;
        }

        /// <summary>
        /// Step 3: Processes a list of pipetting steps to generate a chronological list of liquid transfers.
        /// </summary>
        private List<LiquidTransferEvent> CreateLiquidTransferEvents(List<PipettingStep> steps)
        {
            var transfers = new List<LiquidTransferEvent>();
            // Use a dictionary to track the state of each channel (1 through 8)
            var channelStates = new Dictionary<int, LiquidTransferEvent>();

            foreach (var step in steps.OrderBy(s => s.StartTime))
            {
                foreach (var action in step.ChannelActions)
                {
                    if (!channelStates.ContainsKey(action.ChannelNumber))
                    {
                        // Initialize state for a new channel
                        channelStates[action.ChannelNumber] = new LiquidTransferEvent { ChannelId = action.ChannelNumber };
                    }

                    var state = channelStates[action.ChannelNumber];

                    switch (step.ActionType)
                    {
                        case PipettingActionType.PickupTip:
                            state.TipLabwareId = action.LabwareId;
                            state.TipPositionId = int.TryParse(action.PositionId, out var pos) ? pos : 0;
                            break;

                        case PipettingActionType.Aspirate:
                            state.SourceLabware = action.LabwareId;
                            state.SourcePositionId = action.PositionId;
                            state.Volume = action.Volume;
                            break;

                        case PipettingActionType.Dispense:
                            // A dispense action completes the transfer.
                            state.Timestamp = step.StartTime; // Use the dispense time as the event time
                            state.TargetLabware = action.LabwareId;
                            state.TargetPositionId = action.PositionId;

                            // Add a copy of the completed event to our results
                            transfers.Add(new LiquidTransferEvent
                            {
                                Timestamp = state.Timestamp,
                                ChannelId = state.ChannelId,
                                SourceLabware = state.SourceLabware,
                                SourcePositionId = state.SourcePositionId,
                                TargetLabware = state.TargetLabware,
                                TargetPositionId = state.TargetPositionId,
                                Volume = state.Volume,
                                TipLabwareId = state.TipLabwareId,
                                TipPositionId = state.TipPositionId
                            });

                            // Partially reset the state for this channel, keeping the tip info
                            state.SourceLabware = null;
                            state.SourcePositionId = null;
                            state.Volume = 0;
                            break;

                        case PipettingActionType.EjectTip:
                            // Reset the tip info for this channel
                            state.TipLabwareId = null;
                            state.TipPositionId = 0;
                            break;
                    }
                }
            }

            return transfers;
        }

        /// <summary>
        /// Helper to parse the details string from a "complete" line based on the action type.
        /// </summary>
        private List<ChannelAction> ParseChannelDetails(string details, PipettingActionType actionType)
        {
            var actions = new List<ChannelAction>();

            // Use the appropriate Regex based on whether the action involves volume
            bool isVolumeAction = actionType == PipettingActionType.Aspirate || actionType == PipettingActionType.Dispense;
            var regex = isVolumeAction ? PipettingDetailsRegex : TipActionDetailsRegex;

            var matches = regex.Matches(details);
            foreach (Match match in matches)
            {
                actions.Add(new ChannelAction
                {
                    ChannelNumber = int.Parse(match.Groups["channel"].Value),
                    LabwareId = match.Groups["labware"].Value.Trim(),
                    PositionId = match.Groups["position"].Value.Trim(),
                    // Only parse volume if it's expected for the action type
                    Volume = isVolumeAction ? double.Parse(match.Groups["volume"].Value, CultureInfo.InvariantCulture) : 0
                });
            }
            return actions;
        }

        /// <summary>
        /// Checks if a command string corresponds to a known pipetting action.
        /// </summary>
        private bool IsPipettingCommand(string command)
        {
            return command.Contains("Aspirate") || command.Contains("Dispense") || command.Contains("Tip Pick Up") || command.Contains("Tip Eject");
        }

        /// <summary>
        /// Maps a command string to a PipettingActionType enum.
        /// </summary>
        private PipettingActionType GetPipettingActionType(string command)
        {
            if (command.Contains("Aspirate")) return PipettingActionType.Aspirate;
            if (command.Contains("Dispense")) return PipettingActionType.Dispense;
            if (command.Contains("Tip Pick Up")) return PipettingActionType.PickupTip;
            if (command.Contains("Tip Eject")) return PipettingActionType.EjectTip;
            if (command.Contains("Initialize")) return PipettingActionType.Initialize;
            return PipettingActionType.Unknown;
        }
    }
}

