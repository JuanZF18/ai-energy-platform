namespace EnergyPlatform.Domain.Meters;

public sealed record ConsumptionSnapshot(
    double BaselineDailyKwh,
    double CurrentDailyKwh,
    IReadOnlyList<double> HourlyBaseline);
