namespace EnergyPlatform.Domain.Readings;

public sealed class Reading
{
    private Reading()
    {
    }

    public Reading(string meterId, DateTime timestamp, double consumptionKwh, double voltageV, double currentA, double powerFactor)
    {
        MeterId = meterId;
        Timestamp = timestamp;
        ConsumptionKwh = consumptionKwh;
        VoltageV = voltageV;
        CurrentA = currentA;
        PowerFactor = powerFactor;
    }

    public long Id { get; private set; }
    public string MeterId { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public double ConsumptionKwh { get; private set; }
    public double VoltageV { get; private set; }
    public double CurrentA { get; private set; }
    public double PowerFactor { get; private set; }
    public ReadingStatus Status { get; private set; } = ReadingStatus.Ok;

    public int HourOfDay => Timestamp.Hour;

    public double ElectricalRatio
    {
        get
        {
            var apparentEnergyKwh = VoltageV * CurrentA * PowerFactor / 1000;
            return apparentEnergyKwh > 0 ? ConsumptionKwh / apparentEnergyKwh : double.NaN;
        }
    }
}
