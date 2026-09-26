namespace EnergyPlatform.Domain.Analysis;

public sealed record AnalysisOptions
{
    public static AnalysisOptions Default { get; } = new();

    public double DeviationThreshold { get; init; } = 0.25;
    public int MinimumShiftHours { get; init; } = 3;
    public double RobustZThreshold { get; init; } = 5;
    public double NominalVoltage { get; init; } = 220;
    public double VoltageTolerance { get; init; } = 0.05;
    public int MinimumSuspectReadings { get; init; } = 3;
    public TimeSpan EventTolerance { get; init; } = TimeSpan.FromHours(2);
    public double PowerFactorDegradation { get; init; } = 0.08;
    public double HighSeverityDeviation { get; init; } = 0.5;

    public double MinimumVoltage => NominalVoltage * (1 - VoltageTolerance);
    public double MaximumVoltage => NominalVoltage * (1 + VoltageTolerance);
}
