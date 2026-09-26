using EnergyPlatform.Domain.Analysis;
using EnergyPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlatform.Infrastructure.Analysis;

public sealed class AnalysisInputLoader(EnergyDbContext db)
{
    public async Task<IReadOnlyList<MeterAnalysis>> LoadAsync(CancellationToken cancellationToken)
    {
        var readings = await db.Readings.AsNoTracking().ToListAsync(cancellationToken);
        var events = await db.Events.AsNoTracking().ToListAsync(cancellationToken);
        var eventsByMeter = events.ToLookup(meterEvent => meterEvent.MeterId);

        return
        [
            .. readings
                .GroupBy(reading => reading.MeterId)
                .OrderBy(meter => meter.Key)
                .Select(meter => new MeterAnalysis(meter.Key, meter, eventsByMeter[meter.Key]))
        ];
    }
}
