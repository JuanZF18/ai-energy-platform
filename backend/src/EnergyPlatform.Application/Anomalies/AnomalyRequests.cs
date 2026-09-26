using System.ComponentModel.DataAnnotations;
using EnergyPlatform.Domain.Anomalies;

namespace EnergyPlatform.Application.Anomalies;

public sealed record AnomalyListQuery(AnomalyType? Type, AnomalyStatus? Status, int? Limit);

public sealed record UpdateAnomalyStatusRequest
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public required AnomalyStatus Status { get; init; }

    [MaxLength(500, ErrorMessage = "La nota puede tener máximo 500 caracteres.")]
    public string? Note { get; init; }
}
