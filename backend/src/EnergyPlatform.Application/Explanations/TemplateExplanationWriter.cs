using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Application.Explanations;

public sealed class TemplateExplanationWriter : IExplanationWriter
{
    public Task<WrittenExplanation> WriteAsync(Finding finding, CancellationToken cancellationToken)
    {
        var explanation = new Explanation(
            finding.Reason,
            [.. finding.Evidence.ChangedVariables.Select(Describe)],
            PossibleCauses(finding),
            NextSteps(finding.Type));

        return Task.FromResult(new WrittenExplanation(explanation, ExplanationSource.Template));
    }

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
            "Un problema en la instalación eléctrica que afecta el factor de potencia"
        ],
        AnomalyType.DataQuality =>
        [
            "Una falla intermitente del medidor o de sus transformadores de medida",
            "Problemas de comunicación que corrompen algunas lecturas",
            "Una calibración incorrecta del equipo"
        ],
        AnomalyType.ExplainableAnomaly =>
        [
            .. finding.Evidence.Events.Where(item => item.ExplainsChange).Select(item => $"El evento registrado: \"{item.Description}\""),
            "Conviene confirmar que la magnitud del cambio es la esperada para ese evento"
        ],
        _ => [.. finding.Evidence.Events.Where(item => item.ExplainsChange).Select(item => $"El evento registrado: \"{item.Description}\"")]
    };

    private static IReadOnlyList<string> NextSteps(AnomalyType type) => type switch
    {
        AnomalyType.RealAnomaly =>
        [
            "Enviar a mantenimiento a revisar la carga conectada y el estado del medidor",
            "Confirmar con operación si hubo un cambio que no se reportó",
            "Revisar la compensación de energía reactiva"
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
            "Ajustar el baseline del medidor al nuevo nivel de consumo"
        ],
        _ =>
        [
            "No escalar",
            "Mantener al día el registro de paradas programadas"
        ]
    };
}
