using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

[Flags]
public enum SuspicionReason
{
    None = 0,
    VoltageOutOfRange = 1,
    AtypicalPowerFactor = 2,
    InconsistentElectricalRatio = 4
}

public sealed record SuspectReading(Reading Reading, SuspicionReason Reasons);
