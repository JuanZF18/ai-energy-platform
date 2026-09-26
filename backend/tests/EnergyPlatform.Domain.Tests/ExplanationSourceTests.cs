using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Domain.Tests;

public sealed class ExplanationSourceTests
{
    private static readonly Explanation AnyExplanation = new("Resumen", ["Cambio"], ["Causa"], ["Paso"]);

    [Fact]
    public void Asks_for_an_explanation_when_the_anomaly_has_none()
    {
        Assert.True(NewAnomaly().NeedsExplanationFrom(ExplanationSource.LanguageModel));
    }

    [Theory]
    [InlineData(ExplanationSource.LanguageModel, ExplanationSource.Template, true)]
    [InlineData(ExplanationSource.Template, ExplanationSource.LanguageModel, true)]
    [InlineData(ExplanationSource.LanguageModel, ExplanationSource.LanguageModel, false)]
    [InlineData(ExplanationSource.Template, ExplanationSource.Template, false)]
    public void Rewrites_the_explanation_only_when_the_active_source_changes(ExplanationSource stored, ExplanationSource active, bool expected)
    {
        var anomaly = NewAnomaly();
        anomaly.AttachExplanation(AnyExplanation, stored);

        Assert.Equal(expected, anomaly.NeedsExplanationFrom(active));
    }

    private static Anomaly NewAnomaly()
    {
        var finding = new AnomalyEngine(AnalysisOptions.Default).Analyze(ChallengeData.MetersToAnalyze())[0];
        return Anomaly.Detect(finding, "hash", Guid.NewGuid(), DateTimeOffset.UtcNow);
    }
}
