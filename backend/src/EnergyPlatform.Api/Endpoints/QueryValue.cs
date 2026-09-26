namespace EnergyPlatform.Api.Endpoints;

public static class QueryValue
{
    public static bool TryParse<TEnum>(string? value, TEnum fallback, out TEnum result) where TEnum : struct, Enum
    {
        result = fallback;
        return string.IsNullOrWhiteSpace(value) || Enum.TryParse(value.Replace("_", string.Empty), ignoreCase: true, out result);
    }

    public static bool TryParseOptional<TEnum>(string? value, out TEnum? result) where TEnum : struct, Enum
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var parsed = Enum.TryParse(value.Replace("_", string.Empty), ignoreCase: true, out TEnum parsedValue);
        result = parsed ? parsedValue : null;
        return parsed;
    }

    public static Dictionary<string, string[]> InvalidValue<TEnum>(string parameter) where TEnum : struct, Enum =>
        new() { [parameter] = [$"Valores permitidos: {string.Join(", ", Enum.GetNames<TEnum>())}."] };
}
