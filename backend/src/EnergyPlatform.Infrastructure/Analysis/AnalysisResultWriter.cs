using System.Security.Cryptography;
using System.Text;
using EnergyPlatform.Application.Explanations;
using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Domain.Meters;
using EnergyPlatform.Domain.Readings;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Analysis;

public sealed class AnalysisResultWriter(EnergyDbContext db, IExplanationWriter explanationWriter, TimeProvider clock)
{
    public async Task<IReadOnlyList<Anomaly>> RecordAnomaliesAsync(Guid analysisRunId, IReadOnlyList<Finding> findings, CancellationToken cancellationToken)
    {
        var fingerprints = findings.Select(finding => finding.Fingerprint).ToList();
        var existing = await db.Anomalies
            .Where(anomaly => fingerprints.Contains(anomaly.Fingerprint))
            .ToDictionaryAsync(anomaly => anomaly.Fingerprint, cancellationToken);

        var anomalies = new List<Anomaly>();
        foreach (var finding in findings)
        {
            var anomaly = Upsert(finding, existing.GetValueOrDefault(finding.Fingerprint), analysisRunId);
            if (anomaly.NeedsExplanationFrom(explanationWriter.PreferredSource))
            {
                var written = await explanationWriter.WriteAsync(finding, cancellationToken);
                anomaly.AttachExplanation(written.Explanation, written.Source);
            }

            anomalies.Add(anomaly);
        }

        return anomalies;
    }

    public async Task ApplyToMetersAsync(IReadOnlyList<MeterAnalysis> analyses, IReadOnlyList<Anomaly> anomalies, CancellationToken cancellationToken)
    {
        var meters = await db.Meters.ToDictionaryAsync(meter => meter.MeterId, cancellationToken);

        foreach (var analysis in analyses)
        {
            if (meters.TryGetValue(analysis.MeterId, out var meter))
            {
                meter.RecordConsumption(analysis.RequiredSnapshot);
                meter.ChangeStatus(MeterStatusPolicy.From(anomalies.Where(anomaly => anomaly.MeterId == meter.MeterId)));
            }
        }

        await MarkSuspectReadingsAsync(analyses, cancellationToken);
    }

    private Anomaly Upsert(Finding finding, Anomaly? existing, Guid analysisRunId)
    {
        var evidenceHash = HashOf(finding.Evidence);
        if (existing is not null)
        {
            existing.Refresh(finding, evidenceHash, analysisRunId, clock.GetUtcNow());
            return existing;
        }

        var detected = Anomaly.Detect(finding, evidenceHash, analysisRunId, clock.GetUtcNow());
        db.Anomalies.Add(detected);
        return detected;
    }

    private async Task MarkSuspectReadingsAsync(IReadOnlyList<MeterAnalysis> analyses, CancellationToken cancellationToken)
    {
        await db.Readings
            .Where(reading => reading.Status == ReadingStatus.Suspect)
            .ExecuteUpdateAsync(setters => setters.SetProperty(reading => reading.Status, ReadingStatus.Ok), cancellationToken);

        foreach (var analysis in analyses.Where(item => item.DataQualityIssue is not null))
        {
            var timestamps = analysis.DataQualityIssue!.Readings.Select(suspect => suspect.Reading.Timestamp).ToList();
            await db.Readings
                .Where(reading => reading.MeterId == analysis.MeterId && timestamps.Contains(reading.Timestamp))
                .ExecuteUpdateAsync(setters => setters.SetProperty(reading => reading.Status, ReadingStatus.Suspect), cancellationToken);
        }
    }

    private static string HashOf(AnomalyEvidence evidence) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonColumn.Serialize(evidence))));
}
