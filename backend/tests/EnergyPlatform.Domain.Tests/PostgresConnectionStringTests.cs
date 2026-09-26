using EnergyPlatform.Infrastructure.Persistence;
using Npgsql;

namespace EnergyPlatform.Domain.Tests;

public sealed class PostgresConnectionStringTests
{
    [Fact]
    public void Converts_a_neon_url_into_npgsql_settings()
    {
        var normalized = PostgresConnectionString.Normalize(
            "postgresql://owner:p%40ss@ep-sample-123.us-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require");

        var settings = new NpgsqlConnectionStringBuilder(normalized);

        Assert.Equal("ep-sample-123.us-east-1.aws.neon.tech", settings.Host);
        Assert.Equal(5432, settings.Port);
        Assert.Equal("neondb", settings.Database);
        Assert.Equal("owner", settings.Username);
        Assert.Equal("p@ss", settings.Password);
        Assert.Equal(SslMode.Require, settings.SslMode);
        Assert.Equal(ChannelBinding.Require, settings.ChannelBinding);
    }

    [Fact]
    public void Keeps_a_key_value_connection_string_as_it_is()
    {
        const string keyValue = "Host=localhost;Database=energy;Username=postgres;Password=secret";

        Assert.Equal(keyValue, PostgresConnectionString.Normalize(keyValue));
    }
}
