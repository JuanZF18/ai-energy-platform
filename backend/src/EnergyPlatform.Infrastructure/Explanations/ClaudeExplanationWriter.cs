using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using EnergyPlatform.Application.Explanations;
using EnergyPlatform.Domain.Anomalies;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyPlatform.Infrastructure.Explanations;

public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public bool Enabled { get; set; } = true;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-5";
    public int MaxTokens { get; set; } = 4000;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    public bool IsConfigured => Enabled && !string.IsNullOrWhiteSpace(ApiKey);
}

public sealed class ClaudeExplanationWriter(
    AnthropicClient client,
    IOptions<AnthropicOptions> options,
    TemplateExplanationWriter fallback,
    ILogger<ClaudeExplanationWriter> logger) : IExplanationWriter
{
    private const int MaxItemsPerList = 4;

    private bool isUnavailable;

    public ExplanationSource PreferredSource => ExplanationSource.LanguageModel;

    private const string Instructions = """
        Eres analista de operaciones de una empresa de gestión de energía en Colombia. Escribes para el
        cliente: jefes de planta y técnicos de mantenimiento que no son expertos en estadística. Explicas una
        anomalía que un motor estadístico ya detectó y clasificó en un medidor de energía. Tu texto debe
        dejar claro, en pocos segundos, qué pasó, qué tan grave es y qué hacer.

        Reglas:
        - La clasificación, la severidad y la confianza del motor son definitivas. No las cambies ni las discutas.
        - Usa solo las cifras que aparecen en los datos que recibes. No calcules cifras nuevas ni inventes
          valores, fechas o eventos. Si una cifra no está en los datos, describe el hecho sin número.
        - Cuando cites un porcentaje, di qué mide: "variación diaria" (variationPercent: consumo del último
          día frente al esperado) o "en promedio X% por hora" (meanHourlyDeviationPercent: promedio hora a
          hora durante el cambio). No los mezcles ni los presentes como la misma cifra.
        - No escribas la palabra "baseline" ni otros términos en inglés, tampoco entre paréntesis.
        - La confianza va como porcentaje: 0.95 se escribe "95%".
        - Escribe en español neutro, frases cortas y concretas, sin jerga estadística ni palabras en inglés.
          Usa estos nombres y nunca los códigos en inglés:
          REAL_ANOMALY = anomalía real · DATA_QUALITY = problema de calidad de datos ·
          EXPLAINABLE_ANOMALY = anomalía explicable · FALSE_POSITIVE = falso positivo ·
          HIGH/MEDIUM/LOW = severidad alta/media/baja · baseline = consumo esperado ·
          OPERATIONAL_CHANGE = cambio operativo · SCHEDULED_OUTAGE = parada programada ·
          UNKNOWN = registro sin evento operativo. Si citas la descripción de un evento, tradúcela al español.
        - Números con coma decimal (47,6%) y fechas como dd/MM h:mm AM/PM (por ejemplo 12/09 2:00 PM).

        Qué devolver:
        - summary: dos o tres frases. La primera dice qué pasó en el medidor, con la cifra principal. La
          segunda dice por qué importa para el cliente (costo, riesgo o datos poco confiables); en un falso
          positivo, di por qué no representa un problema. Si aplica, una tercera con lo primero que conviene
          hacer, coherente con nextSteps (no digas que no hay nada que hacer si luego listas pasos).
        - possibleCauses: de una a cuatro causas físicas u operativas plausibles, de la más a la menos
          probable. Si hay un evento registrado que explica el cambio, nómbralo primero. La ausencia de un
          evento no es una causa: no la listes (ya se muestra en la evidencia).
        - nextSteps: de una a cuatro acciones concretas, en el orden en que se deben hacer, coherentes con
          la acción recomendada por el motor. Empieza cada una con un verbo.
        """;

    private static readonly JsonSerializerOptions ReadableJson = new(JsonColumn.SerializerOptions)
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private static readonly Dictionary<string, JsonElement> Schema = new()
    {
        ["type"] = JsonSerializer.SerializeToElement("object"),
        ["properties"] = JsonSerializer.SerializeToElement(new
        {
            summary = new { type = "string" },
            possibleCauses = new { type = "array", items = new { type = "string" } },
            nextSteps = new { type = "array", items = new { type = "string" } }
        }),
        ["required"] = JsonSerializer.SerializeToElement(new[] { "summary", "possibleCauses", "nextSteps" }),
        ["additionalProperties"] = JsonSerializer.SerializeToElement(false)
    };

    public async Task<WrittenExplanation> WriteAsync(Finding finding, CancellationToken cancellationToken)
    {
        if (isUnavailable)
        {
            return await fallback.WriteAsync(finding, cancellationToken);
        }

        try
        {
            var explanation = await AskClaudeAsync(finding, cancellationToken);
            if (explanation is not null)
            {
                return new WrittenExplanation(explanation, ExplanationSource.LanguageModel);
            }
        }
        catch (AnthropicRateLimitException exception)
        {
            MarkUnavailable(exception, "Claude limitó las peticiones", finding);
        }
        catch (Exception exception) when (exception is AnthropicUnauthorizedException or AnthropicForbiddenException or Anthropic5xxException)
        {
            MarkUnavailable(exception, "Claude no está disponible", finding);
        }
        catch (AnthropicApiException exception)
        {
            logger.LogWarning(exception, "Claude respondió con error; la anomalía {Fingerprint} usa la plantilla.", finding.Fingerprint);
        }
        catch (AnthropicIOException exception)
        {
            MarkUnavailable(exception, "No se pudo contactar a Claude", finding);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            MarkUnavailable(exception, "Claude tardó demasiado", finding);
        }

        return await fallback.WriteAsync(finding, cancellationToken);
    }

    private void MarkUnavailable(Exception exception, string problem, Finding finding)
    {
        isUnavailable = true;
        logger.LogWarning(exception, "{Problem}; la anomalía {Fingerprint} y las siguientes de este análisis usan la plantilla.", problem, finding.Fingerprint);
    }

    private async Task<Explanation?> AskClaudeAsync(Finding finding, CancellationToken cancellationToken)
    {
        var evidenceJson = JsonSerializer.Serialize(new
        {
            finding.MeterId,
            finding.Type,
            finding.Severity,
            Confidence = Math.Round(finding.Confidence, 2),
            finding.Reason,
            finding.RecommendedAction,
            finding.Evidence
        }, ReadableJson);

        var response = await client.Messages.Create(new MessageCreateParams
        {
            Model = options.Value.Model,
            MaxTokens = options.Value.MaxTokens,
            System = Instructions,
            OutputConfig = new OutputConfig
            {
                Effort = Effort.Low,
                Format = new JsonOutputFormat { Schema = Schema }
            },
            Messages = [new() { Role = Role.User, Content = $"Datos de la anomalía (JSON):\n{evidenceJson}" }]
        }, cancellationToken);

        if (response.StopReason != StopReason.EndTurn)
        {
            logger.LogWarning("Claude terminó con {StopReason}; la anomalía {Fingerprint} usa la plantilla.", response.StopReason, finding.Fingerprint);
            return null;
        }

        var json = string.Concat(response.Content.Select(block => block.Value).OfType<TextBlock>().Select(block => block.Text));
        var draft = JsonSerializer.Deserialize<Draft>(json, JsonColumn.SerializerOptions);
        var template = await fallback.WriteAsync(finding, cancellationToken);
        var explanation = draft is null ? null : Clean(draft, template.Explanation.WhatChanged);
        if (explanation is null)
        {
            logger.LogWarning("Claude devolvió una explicación incompleta; la anomalía {Fingerprint} usa la plantilla.", finding.Fingerprint);
            return null;
        }

        var unknown = EvidenceNumbers.From(finding, evidenceJson).Unknown(
            [explanation.Summary, .. explanation.PossibleCauses, .. explanation.NextSteps]);
        if (unknown.Count > 0)
        {
            logger.LogWarning(
                "Claude citó cifras que no están en la evidencia ({Numbers}); la anomalía {Fingerprint} usa la plantilla.",
                string.Join(", ", unknown), finding.Fingerprint);
            return null;
        }

        logger.LogInformation(
            "Claude redactó la explicación de {Fingerprint} ({InputTokens} tokens de entrada, {OutputTokens} de salida).",
            finding.Fingerprint, response.Usage.InputTokens, response.Usage.OutputTokens);
        return explanation;
    }

    private static Explanation? Clean(Draft draft, IReadOnlyList<string> whatChanged)
    {
        var causes = Items(draft.PossibleCauses);
        var nextSteps = Items(draft.NextSteps);
        if (string.IsNullOrWhiteSpace(draft.Summary) || causes.Count == 0 || nextSteps.Count == 0)
        {
            return null;
        }

        return new Explanation(draft.Summary.Trim(), whatChanged, causes, nextSteps);
    }

    private static List<string> Items(IReadOnlyList<string>? items) =>
        [.. (items ?? []).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).Take(MaxItemsPerList)];

    private sealed record Draft(
        string? Summary,
        IReadOnlyList<string>? PossibleCauses,
        IReadOnlyList<string>? NextSteps);
}
