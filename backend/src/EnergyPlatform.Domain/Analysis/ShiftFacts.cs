using EnergyPlatform.Domain.Events;
using static EnergyPlatform.Domain.Analysis.SpanishText;

namespace EnergyPlatform.Domain.Analysis;

public static class ShiftFacts
{
    public static IReadOnlyList<string> Describe(
        ConsumptionShift shift,
        ElectricalChange change,
        IReadOnlyList<RelatedEvent> events,
        AnalysisOptions options)
    {
        var side = shift.Direction == ShiftDirection.Up ? "por encima" : "por debajo";
        var facts = new List<string>
        {
            $"{shift.Hours} horas seguidas {side} de lo esperado, en promedio {SignedPercent(shift.MeanDeviation * 100)} por hora",
            shift.IsOngoing
                ? $"El cambio empieza el {Moment(shift.Start)} y sigue activo en la última lectura"
                : $"El cambio va del {Moment(shift.Start)} al {Moment(shift.End)} y luego el consumo vuelve a lo normal",
            change.CurrentFollowsConsumption
                ? $"La corriente cambia {SignedPercent(change.CurrentChange * 100)} junto con el consumo, así que no es un error del contador"
                : $"La corriente cambia {SignedPercent(change.CurrentChange * 100)}, distinto al consumo",
            change.ShowsDegradation(options)
                ? $"El factor de potencia cae de {Number(change.Before.PowerFactor, 2)} a {Number(change.During.PowerFactor, 2)}"
                : $"El factor de potencia se mantiene estable ({Number(change.Before.PowerFactor, 2)} → {Number(change.During.PowerFactor, 2)})"
        };

        facts.AddRange(events.Select(Describe));

        if (events.Count == 0)
        {
            facts.Add("No hay eventos operativos registrados cerca del cambio");
        }

        return facts;
    }

    private static string Describe(RelatedEvent related) => related.Relation == EventRelation.ExplainsChange
        ? $"Coincide con {related.Event.Type.WithIndefiniteArticle()} del {Moment(related.Event.Timestamp)}: \"{related.Event.Description}\""
        : $"{Capitalized(related.Event.Type.WithDefiniteArticle())} del {Moment(related.Event.Timestamp)} (\"{related.Event.Description}\") no explica el cambio";
}
