namespace LogisticsAssetTracker.Api.Common;

// Document 05 "Configurable Thresholds": defaults must not be hard-coded inside business logic.
public class MovementThresholdsOptions
{
    public const string SectionName = "MovementThresholds";

    public int HighUpdateFrequencyCount { get; set; } = 5;
    public int HighUpdateFrequencyWindowMinutes { get; set; } = 60;
    public int RepeatedLostReactivationCount { get; set; } = 2;
    public int RepeatedLostReactivationWindowHours { get; set; } = 24;
    public int ConflictingLocationWindowMinutes { get; set; } = 10;

    // Phase 5 risk thresholds (Document 05 Risk Level Assignment).
    public int MaxInTransitHours { get; set; } = 72;
    public int HighDelayRiskDays { get; set; } = 7;
    public int StaleAssetDays { get; set; } = 7;
    public int HighStaleDays { get; set; } = 14;
}
