using System.Globalization;
using System.Text.RegularExpressions;
using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Infrastructure.Explanations;

public sealed partial class EvidenceNumbers
{
    private const int SmallestUncheckedNumber = 5;
    private readonly List<double> allowed;

    private EvidenceNumbers(List<double> allowed) => this.allowed = allowed;

    public static EvidenceNumbers From(Finding finding, string evidenceJson)
    {
        var sources = new[] { finding.MeterId, finding.Reason, finding.RecommendedAction, evidenceJson };
        var numbers = sources.SelectMany(source => NumberPattern().Matches(source))
            .SelectMany(match => Readings(match.Value))
            .ToList();

        numbers.Add(Math.Round(finding.Confidence * 100, 1));
        return new EvidenceNumbers(numbers);
    }

    public IReadOnlyList<string> Unknown(IEnumerable<string> texts) =>
    [
        .. texts.SelectMany(text => NumberPattern().Matches(text))
            .Select(match => match.Value)
            .Where(value => !Readings(value).Any(IsKnown))
            .Distinct()
    ];

    private bool IsKnown(double value) =>
        Math.Abs(value) <= SmallestUncheckedNumber ||
        allowed.Any(known => Math.Abs(Math.Abs(known) - Math.Abs(value)) <= Math.Max(0.051, Math.Abs(known) * 0.005));

    private static IEnumerable<double> Readings(string token)
    {
        var spanish = token.Replace(".", string.Empty).Replace(',', '.');
        var english = token.Replace(",", string.Empty);
        foreach (var candidate in new[] { spanish, english, token.Replace(',', '.') })
        {
            if (double.TryParse(candidate, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                yield return value;
            }
        }
    }

    [GeneratedRegex(@"\d+(?:[.,]\d+)*")]
    private static partial Regex NumberPattern();
}
