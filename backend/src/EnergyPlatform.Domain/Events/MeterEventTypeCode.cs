namespace EnergyPlatform.Domain.Events;

public static class MeterEventTypeCode
{
    private static readonly Dictionary<MeterEventType, string> Codes = new()
    {
        [MeterEventType.OperationalChange] = "OPERATIONAL_CHANGE",
        [MeterEventType.ScheduledOutage] = "SCHEDULED_OUTAGE",
        [MeterEventType.DataQuality] = "DATA_QUALITY",
        [MeterEventType.Unknown] = "UNKNOWN"
    };

    public static string ToCode(this MeterEventType type) => Codes[type];

    public static MeterEventType FromCode(string code) =>
        Codes.FirstOrDefault(pair => pair.Value.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase)).Key;
}
