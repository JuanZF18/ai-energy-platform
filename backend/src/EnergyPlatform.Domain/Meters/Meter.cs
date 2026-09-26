namespace EnergyPlatform.Domain.Meters;

public sealed class Meter
{
    private Meter()
    {
    }

    public Meter(string meterId, string name, string location, DateTimeOffset createdAt)
    {
        MeterId = meterId;
        Name = name;
        Location = location;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string MeterId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public MeterStatus Status { get; private set; } = MeterStatus.NotAnalyzed;
    public double CurrentDailyKwh { get; private set; }
    public double? BaselineDailyKwh { get; private set; }
    public double? VariationPercent { get; private set; }
    public double[] HourlyBaseline { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }

    public void RecordCurrentConsumption(double currentDailyKwh) => CurrentDailyKwh = currentDailyKwh;

    public void RecordConsumption(ConsumptionSnapshot snapshot)
    {
        CurrentDailyKwh = snapshot.CurrentDailyKwh;
        BaselineDailyKwh = snapshot.BaselineDailyKwh;
        HourlyBaseline = [.. snapshot.HourlyBaseline];
        VariationPercent = snapshot.BaselineDailyKwh > 0
            ? (snapshot.CurrentDailyKwh - snapshot.BaselineDailyKwh) / snapshot.BaselineDailyKwh * 100
            : null;
    }

    public void ChangeStatus(MeterStatus status) => Status = status;
}
