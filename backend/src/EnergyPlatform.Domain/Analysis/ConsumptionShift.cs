namespace EnergyPlatform.Domain.Analysis;

public enum ShiftDirection
{
    Up,
    Down
}

public sealed record ConsumptionShift(
    DateTime Start,
    DateTime End,
    int Hours,
    ShiftDirection Direction,
    double MeanDeviation,
    bool IsOngoing)
{
    public bool Contains(DateTime timestamp) => timestamp >= Start && timestamp <= End;
}
