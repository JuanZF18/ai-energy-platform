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

    private static readonly Dictionary<MeterEventType, (string Indefinite, string Definite)> SpanishNames = new()
    {
        [MeterEventType.OperationalChange] = ("un cambio operativo", "el cambio operativo"),
        [MeterEventType.ScheduledOutage] = ("una parada programada", "la parada programada"),
        [MeterEventType.DataQuality] = ("un reporte de falla de datos", "el reporte de falla de datos"),
        [MeterEventType.Unknown] = ("un registro sin evento operativo", "el registro sin evento operativo")
    };

    public static string ToCode(this MeterEventType type) => Codes[type];

    public static string WithIndefiniteArticle(this MeterEventType type) => SpanishNames[type].Indefinite;

    public static string WithDefiniteArticle(this MeterEventType type) => SpanishNames[type].Definite;

    public static MeterEventType FromCode(string code) =>
        Codes.Where(pair => pair.Value.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase)).Select(pair => pair.Key).DefaultIfEmpty(MeterEventType.Unknown).First();
}
