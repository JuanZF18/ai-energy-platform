using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyPlatform.Infrastructure.Persistence;

public static class JsonColumn
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static PropertyBuilder<T> StoredAsJson<T>(this PropertyBuilder<T> property)
    {
        var comparer = new ValueComparer<T>(
            (left, right) => Serialize(left) == Serialize(right),
            value => Serialize(value).GetHashCode(),
            value => Deserialize<T>(Serialize(value)));

        property
            .HasConversion(value => Serialize(value), json => Deserialize<T>(json), comparer)
            .HasColumnType("jsonb");

        return property;
    }

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, SerializerOptions);

    private static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, SerializerOptions)!;
}
