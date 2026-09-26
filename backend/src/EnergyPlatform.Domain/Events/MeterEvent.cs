using System.Text.RegularExpressions;

namespace EnergyPlatform.Domain.Events;

public sealed partial class MeterEvent
{
    private MeterEvent()
    {
    }

    public MeterEvent(string meterId, DateTime timestamp, MeterEventType type, string description)
    {
        MeterId = meterId;
        Timestamp = timestamp;
        Type = type;
        Description = description;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string MeterId { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public MeterEventType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public TimeSpan? DeclaredDuration
    {
        get
        {
            var match = DurationInHours().Match(Description);
            return match.Success ? TimeSpan.FromHours(int.Parse(match.Groups[1].Value)) : null;
        }
    }

    [GeneratedRegex(@"(\d+)\s*hours?", RegexOptions.IgnoreCase)]
    private static partial Regex DurationInHours();
}
