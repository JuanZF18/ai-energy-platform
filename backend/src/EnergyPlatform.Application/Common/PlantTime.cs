using Microsoft.Extensions.Options;

namespace EnergyPlatform.Application.Common;

public sealed class PlantOptions
{
    public const string SectionName = "Plant";

    public TimeSpan UtcOffset { get; set; } = TimeSpan.FromHours(-5);
}

public sealed class PlantTime(IOptions<PlantOptions> options)
{
    public DateTimeOffset ToOffset(DateTime plantLocalTime) =>
        new(DateTime.SpecifyKind(plantLocalTime, DateTimeKind.Unspecified), options.Value.UtcOffset);

    public DateTime ToPlantLocal(DateTimeOffset moment) =>
        DateTime.SpecifyKind(moment.ToOffset(options.Value.UtcOffset).DateTime, DateTimeKind.Unspecified);
}
