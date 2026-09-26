using System.Globalization;

namespace EnergyPlatform.Domain.Analysis;

public static class SpanishText
{
    private static readonly NumberFormatInfo Numbers = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NegativeSign = "-"
    };

    public static string SignedPercent(double percent) => percent.ToString("+#,##0.0;-#,##0.0;0.0", Numbers) + "%";

    public static string Percent(double percent) => Math.Abs(percent).ToString("#,##0.0", Numbers) + "%";

    public static string Number(double value, int decimals = 0) => value.ToString("N" + decimals, Numbers);

    public static string Moment(DateTime timestamp) => timestamp.ToString("dd/MM HH:mm", CultureInfo.InvariantCulture);

    public static string Day(DateTime timestamp) => timestamp.ToString("dd/MM", CultureInfo.InvariantCulture);

    public static string Capitalized(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];
}
