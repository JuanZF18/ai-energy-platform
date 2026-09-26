namespace EnergyPlatform.Domain.Anomalies;

public static class RecommendedActions
{
    public static string For(AnomalyType type) => type switch
    {
        AnomalyType.RealAnomaly => "Investigar medidor e instalación.",
        AnomalyType.DataQuality => "Validar medidor, comunicaciones y calibración.",
        AnomalyType.ExplainableAnomaly => "Validar con operación que el cambio corresponde al evento y actualizar el consumo esperado.",
        _ => "No escalar."
    };
}
