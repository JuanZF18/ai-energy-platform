using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.AnalysisRuns;

public sealed record AnalysisSummary(
    int MetersAnalyzed,
    int ReadingsAnalyzed,
    int Cases,
    int Anomalies,
    int PriorityCases,
    int RealAnomalies,
    int DataQualityIssues,
    int ExplainableAnomalies,
    int FalsePositives)
{
    public static AnalysisSummary From(IReadOnlyCollection<Finding> findings, int metersAnalyzed, int readingsAnalyzed) => new(
        metersAnalyzed,
        readingsAnalyzed,
        findings.Count,
        findings.Count(finding => finding.Type != AnomalyType.FalsePositive),
        findings.Count(finding => finding.IsPriority),
        findings.Count(finding => finding.Type == AnomalyType.RealAnomaly),
        findings.Count(finding => finding.Type == AnomalyType.DataQuality),
        findings.Count(finding => finding.Type == AnomalyType.ExplainableAnomaly),
        findings.Count(finding => finding.Type == AnomalyType.FalsePositive));
}
