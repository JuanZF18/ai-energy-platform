namespace EnergyPlatform.Application.Anomalies;

public interface IAnomalyService
{
    Task<IReadOnlyList<AnomalyListItemResponse>> ListAsync(AnomalyListQuery query, CancellationToken cancellationToken);

    Task<AnomalyDetailResponse?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<AnomalyDetailResponse?> ChangeStatusAsync(Guid id, UpdateAnomalyStatusRequest request, string changedBy, CancellationToken cancellationToken);
}
