using EnergyPlatform.Domain.Readings;

namespace EnergyPlatform.Domain.Analysis;

public sealed record ElectricalAverages(double ConsumptionKwh, double VoltageV, double CurrentA, double PowerFactor)
{
    public static ElectricalAverages Of(IReadOnlyCollection<Reading> readings) => new(
        readings.Average(reading => reading.ConsumptionKwh),
        readings.Average(reading => reading.VoltageV),
        readings.Average(reading => reading.CurrentA),
        readings.Average(reading => reading.PowerFactor));
}

public sealed record ElectricalChange(ElectricalAverages Before, ElectricalAverages During)
{
    public double ConsumptionChange => Relative(Before.ConsumptionKwh, During.ConsumptionKwh);
    public double CurrentChange => Relative(Before.CurrentA, During.CurrentA);
    public double VoltageChange => Relative(Before.VoltageV, During.VoltageV);
    public double PowerFactorChange => Relative(Before.PowerFactor, During.PowerFactor);
    public double PowerFactorDelta => During.PowerFactor - Before.PowerFactor;

    public bool CurrentFollowsConsumption =>
        Math.Sign(CurrentChange) == Math.Sign(ConsumptionChange)
        && Math.Abs(CurrentChange) >= Math.Abs(ConsumptionChange) / 2;

    public bool ShowsDegradation(AnalysisOptions options) => PowerFactorDelta <= -options.PowerFactorDegradation;

    public static ElectricalChange Around(ConsumptionShift shift, IReadOnlyList<Reading> readings)
    {
        var during = readings.Where(reading => shift.Contains(reading.Timestamp)).ToList();
        var hoursCovered = during.Select(reading => reading.HourOfDay).ToHashSet();
        var sameHoursBefore = readings
            .Where(reading => reading.Timestamp < shift.Start && hoursCovered.Contains(reading.HourOfDay))
            .ToList();

        var before = sameHoursBefore.Count > 0
            ? sameHoursBefore
            : readings.Where(reading => !shift.Contains(reading.Timestamp)).ToList();

        return new ElectricalChange(ElectricalAverages.Of(before), ElectricalAverages.Of(during));
    }

    private static double Relative(double before, double after) => before == 0 ? 0 : after / before - 1;
}
