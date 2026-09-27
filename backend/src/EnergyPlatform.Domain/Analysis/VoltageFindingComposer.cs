using EnergyPlatform.Domain.Anomalies;
using static EnergyPlatform.Domain.Analysis.SpanishText;

namespace EnergyPlatform.Domain.Analysis;

public static class VoltageFindingComposer
{
    public const string RecommendedAction = "Revisar la alimentación eléctrica con el operador de red y proteger los equipos sensibles.";

    public static Finding Compose(MeterAnalysis meter, VoltageIssue issue, AnalysisOptions options)
    {
        var classification = AnomalyClassifier.ClassifyVoltage(issue, options);
        var confidence = Scoring.ConfidenceForVoltage(issue, classification, options);
        var normal = meter.Readings.Except(issue.Readings).ToList();
        var snapshot = meter.RequiredSnapshot;

        var consumption = new ConsumptionEvidence(
            Math.Round(snapshot.BaselineDailyKwh, 1),
            Math.Round(snapshot.CurrentDailyKwh, 1),
            EvidenceParts.Variation(snapshot.CurrentDailyKwh, snapshot.BaselineDailyKwh),
            0);

        var voltage = new VoltageEvidence(
            issue.Readings.Count,
            Math.Round(issue.LowestVoltage, 1),
            Math.Round(issue.HighestVoltage, 1),
            Math.Round(options.MinimumVoltage, 1),
            Math.Round(options.MaximumVoltage, 1));

        var evidence = new AnomalyEvidence(
            new EvidenceWindow(issue.FirstSeen, issue.LastSeen, issue.SpanHours, issue.IsOngoing),
            consumption,
            EvidenceParts.ChangedVariables(normal, issue.Readings),
            [],
            null,
            confidence,
            Facts(issue, voltage, options),
            voltage);

        return new Finding(
            meter.MeterId,
            EvidenceParts.Fingerprint(meter.MeterId, classification.Type, issue.FirstSeen),
            classification.Type,
            classification.Severity,
            confidence.Score,
            Scoring.Priority(classification.Type, classification.Severity, confidence.Score),
            Reason(issue, options),
            RecommendedAction,
            evidence);
    }

    private static string Reason(VoltageIssue issue, AnalysisOptions options) => issue.IsOvervoltage(options)
        ? $"Voltaje de hasta {Number(issue.HighestVoltage, 1)} V desde el {Moment(issue.FirstSeen)}, por encima del máximo de {Number(options.MaximumVoltage)} V que permite la norma NTC 1340."
        : $"Voltaje de apenas {Number(issue.LowestVoltage, 1)} V desde el {Moment(issue.FirstSeen)}, por debajo del mínimo de {Number(options.MinimumVoltage)} V que permite la norma NTC 1340.";

    private static IReadOnlyList<string> Facts(VoltageIssue issue, VoltageEvidence voltage, AnalysisOptions options) =>
    [
        $"{voltage.ReadingsOutsideStandard} {(voltage.ReadingsOutsideStandard == 1 ? "lectura horaria" : "lecturas horarias")} con voltaje fuera del rango {Number(voltage.MinimumAllowed)}–{Number(voltage.MaximumAllowed)} V de la norma NTC 1340",
        $"Voltaje registrado entre {Number(voltage.LowestVoltage, 1)} y {Number(voltage.HighestVoltage, 1)} V, frente a un nominal de {Number(options.NominalVoltage)} V",
        "El consumo cuadra con voltaje × corriente × factor de potencia: el medidor funciona bien y el voltaje fuera de rango es real",
        "Cada lectura es el promedio de una hora, así que el voltaje estuvo fuera de rango de forma sostenida y no fue un pico de segundos"
    ];
}
