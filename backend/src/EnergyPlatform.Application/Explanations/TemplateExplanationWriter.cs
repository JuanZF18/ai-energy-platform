using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Events;

namespace EnergyPlatform.Application.Explanations;

public sealed class TemplateExplanationWriter : IExplanationWriter
{
    private const double PowerFactorDropPercent = -8;

    public ExplanationSource PreferredSource => ExplanationSource.Template;

    public Task<WrittenExplanation> WriteAsync(Finding finding, CancellationToken cancellationToken)
    {
        var explanation = new Explanation(
            Summary(finding),
            [.. finding.Evidence.ChangedVariables.Select(Describe)],
            PossibleCauses(finding),
            NextSteps(finding));

        return Task.FromResult(new WrittenExplanation(explanation, ExplanationSource.Template));
    }

    private static string Summary(Finding finding)
    {
        var confidence = SpanishText.Number(finding.Confidence * 100);
        var impact = finding.Type switch
        {
            AnomalyType.RealAnomaly =>
                "Un cambio sostenido sin causa conocida puede indicar una falla en la instalación o en los equipos, y afecta el costo de energía.",
            AnomalyType.DataQuality =>
                "Los valores eléctricos de este medidor no son confiables, así que sus reportes pueden estar errados aunque el consumo real sea normal.",
            AnomalyType.ExplainableAnomaly =>
                "El cambio tiene una causa operativa registrada; falta confirmar que su magnitud es la esperada.",
            _ => "No representa un problema: la caída corresponde a la parada registrada."
        };

        return $"{finding.Reason} {impact} La IA lo clasifica como {TypeName(finding.Type)} con {confidence}% de confianza.";
    }

    private static string TypeName(AnomalyType type) => type switch
    {
        AnomalyType.RealAnomaly => "anomalía real",
        AnomalyType.DataQuality => "problema de calidad de datos",
        AnomalyType.ExplainableAnomaly => "anomalía explicable",
        _ => "falso positivo"
    };

    private static string Describe(VariableChange change)
    {
        var unit = string.IsNullOrEmpty(change.Unit) ? string.Empty : $" {change.Unit}";
        return $"{change.Variable}: {SpanishText.Number(change.Before, 2)} → {SpanishText.Number(change.After, 2)}{unit} ({SpanishText.SignedPercent(change.ChangePercent)})";
    }

    private static IReadOnlyList<string> PossibleCauses(Finding finding) => finding.Type switch
    {
        AnomalyType.RealAnomaly =>
        [
            "Una carga nueva o un equipo conectado sin reportar",
            "Un equipo con falla que consume más de lo normal",
            .. PowerFactorDropped(finding) ? ["Un problema en la instalación eléctrica que baja el factor de potencia"] : Array.Empty<string>()
        ],
        AnomalyType.DataQuality =>
        [
            "Una falla intermitente del medidor o de sus transformadores de medida",
            "Problemas de comunicación que corrompen algunas lecturas",
            "Una calibración incorrecta del equipo"
        ],
        AnomalyType.ExplainableAnomaly =>
        [
            .. ExplainingEvents(finding),
            Increased(finding)
                ? "Más carga conectada por la nueva operación: la corriente sube junto con el consumo"
                : "Menos carga en operación: la corriente baja junto con el consumo"
        ],
        _ =>
        [
            .. ExplainingEvents(finding),
            "Equipos detenidos durante la parada: la corriente baja junto con el consumo"
        ]
    };

    private static IReadOnlyList<string> NextSteps(Finding finding) => finding.Type switch
    {
        AnomalyType.RealAnomaly =>
        [
            "Enviar a mantenimiento a revisar la carga conectada y el estado del medidor",
            "Confirmar con operación si hubo un cambio que no se reportó",
            .. PowerFactorDropped(finding) ? ["Revisar la compensación de energía reactiva"] : Array.Empty<string>()
        ],
        AnomalyType.DataQuality =>
        [
            "Revisar el medidor y sus conexiones en sitio",
            "Validar las comunicaciones del equipo",
            "Programar una calibración"
        ],
        AnomalyType.ExplainableAnomaly =>
        [
            "Confirmar con operación que el cambio corresponde al evento",
            "Actualizar el consumo esperado del medidor al nuevo nivel"
        ],
        _ =>
        [
            "No escalar",
            "Confirmar en el registro de mantenimiento que la parada coincide con la ventana detectada",
            "Mantener al día el registro de paradas programadas"
        ]
    };

    private static IEnumerable<string> ExplainingEvents(Finding finding) => finding.Evidence.Events
        .Where(item => item.ExplainsChange)
        .Select(item => $"{SpanishText.Capitalized(MeterEventTypeCode.FromCode(item.Type).WithIndefiniteArticle())} del {SpanishText.Moment(item.Timestamp)}: \"{item.Description}\"");

    private static bool Increased(Finding finding) => finding.Evidence.Consumption.MeanHourlyDeviationPercent >= 0;

    private static bool PowerFactorDropped(Finding finding) =>
        finding.Evidence.ChangedVariables.Any(change => change.Variable == "Factor de potencia" && change.ChangePercent <= PowerFactorDropPercent);
}
