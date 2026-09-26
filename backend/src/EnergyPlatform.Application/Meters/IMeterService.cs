namespace EnergyPlatform.Application.Meters;

public interface IMeterService
{
    Task<IReadOnlyList<MeterListItemResponse>> ListAsync(MeterListQuery query, CancellationToken cancellationToken);

    Task<MeterDetailResponse?> GetAsync(string meterId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ReadingPointResponse>?> GetReadingsAsync(string meterId, ReadingsQuery query, CancellationToken cancellationToken);
}
