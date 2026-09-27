namespace EnergyPlatform.Api.Authentication;

public enum AuthMode
{
    Firebase,
    Demo
}

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public AuthMode Mode { get; set; } = AuthMode.Demo;
    public FirebaseAuthOptions Firebase { get; set; } = new();
    public DemoAuthOptions Demo { get; set; } = new();
}

public sealed class FirebaseAuthOptions
{
    public string ProjectId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string AuthDomain { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;

    public string Issuer => $"https://securetoken.google.com/{ProjectId}";
}

public sealed class DemoAuthOptions
{
    public const string Issuer = "vatio-demo";

    public string Name { get; set; } = "Operador demo";
    public string Email { get; set; } = "demo@vatio.app";
    public string Password { get; set; } = "demo1234";
    public string SigningKey { get; set; } = string.Empty;
    public TimeSpan SessionLifetime { get; set; } = TimeSpan.FromHours(12);
}
