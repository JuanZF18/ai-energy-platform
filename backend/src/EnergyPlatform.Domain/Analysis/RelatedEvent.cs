using EnergyPlatform.Domain.Events;

namespace EnergyPlatform.Domain.Analysis;

public enum EventRelation
{
    ExplainsChange,
    ConfirmsIssue,
    DoesNotExplain
}

public sealed record RelatedEvent(MeterEvent Event, EventRelation Relation);
