using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Infrastructure.Explanations;
using EnergyPlatform.Infrastructure.Persistence;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EnergyPlatform.Domain.Tests;

public sealed class EvidenceNumbersTests
{
    private static readonly JsonSerializerOptions ReadableJson = new(JsonColumn.SerializerOptions)
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private static readonly Lazy<EvidenceNumbers> M112Numbers = new(() =>
    {
        var finding = new AnomalyEngine(AnalysisOptions.Default).Analyze(ChallengeData.MetersToAnalyze()).Single(item => item.MeterId == "M-112");
        return EvidenceNumbers.From(finding, JsonSerializer.Serialize(finding, ReadableJson));
    });

    [Theory]
    [InlineData("16 lecturas inconsistentes desde el 13/09 00:00")]
    [InlineData("voltaje fuera del rango 209–231 V")]
    [InlineData("el factor de potencia cae a 0,58")]
    [InlineData("confianza del 95%")]
    [InlineData("variación diaria de +0,5%")]
    public void Accepts_numbers_that_come_from_the_evidence(string text)
    {
        Assert.Empty(M112Numbers.Value.Unknown([text]));
    }

    [Theory]
    [InlineData("el voltaje llegó a 480 V", "480")]
    [InlineData("hay 73 lecturas sospechosas", "73")]
    [InlineData("el consumo subió 312,4%", "312,4")]
    public void Rejects_numbers_that_the_model_made_up(string text, string invented)
    {
        Assert.Equal([invented], M112Numbers.Value.Unknown([text]));
    }
}
