using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public static class ConsumptionSnapshotBuilder
{
    private const int HoursPerDay = 24;

    public static ConsumptionSnapshot Build(
        IReadOnlyList<Reading> readings,
        IReadOnlyList<ConsumptionShift> shifts,
        HourlyProfile detectionProfile)
    {
        var ongoingShift = shifts.Where(shift => shift.IsOngoing).MinBy(shift => shift.Start);
        var readingsBeforeChange = ongoingShift is null
            ? []
            : readings.Where(reading => reading.Timestamp < ongoingShift.Start).ToList();

        var baseline = readingsBeforeChange.Count >= HoursPerDay
            ? HourlyProfile.From(readingsBeforeChange)
            : detectionProfile;

        return new ConsumptionSnapshot(
            baseline.DailyTotal,
            CurrentDailyConsumption(readings),
            baseline.ExpectedByHour);
    }

    public static double CurrentDailyConsumption(IReadOnlyList<Reading> readings) =>
        readings.TakeLast(HoursPerDay).Sum(reading => reading.ConsumptionKwh);
}
