using System.Web;
using Npgsql;

namespace EnergyPlatform.Infrastructure.Persistence;

public static class PostgresConnectionString
{
    private const int DefaultPort = 5432;

    public static string Normalize(string connectionString)
    {
        var value = connectionString.Trim();
        return IsUrl(value) ? FromUrl(new Uri(value)) : value;
    }

    private static bool IsUrl(string value) =>
        value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        || value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase);

    private static string FromUrl(Uri url)
    {
        var credentials = url.UserInfo.Split(':', 2);
        var options = HttpUtility.ParseQueryString(url.Query);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = url.Host,
            Port = url.IsDefaultPort || url.Port <= 0 ? DefaultPort : url.Port,
            Database = Uri.UnescapeDataString(url.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null
        };

        if (options["sslmode"] is { } sslMode && Enum.TryParse<SslMode>(sslMode.Replace("-", string.Empty), ignoreCase: true, out var parsedSslMode))
        {
            builder.SslMode = parsedSslMode;
        }

        if (options["channel_binding"] is { } channelBinding && Enum.TryParse<ChannelBinding>(channelBinding, ignoreCase: true, out var parsedChannelBinding))
        {
            builder.ChannelBinding = parsedChannelBinding;
        }

        return builder.ConnectionString;
    }
}
