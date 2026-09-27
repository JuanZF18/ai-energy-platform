using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EnergyPlatform.Api.Authentication;

public static class AuthenticationSetup
{
    private const string EmailClaim = "email";

    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        if (options.Mode == AuthMode.Firebase && string.IsNullOrWhiteSpace(options.Firebase.ProjectId))
        {
            throw new InvalidOperationException("Falta 'Auth:Firebase:ProjectId' para usar el modo Firebase.");
        }

        var demoSigningKey = DemoSigningKey(options.Demo);
        services.AddSingleton(options);
        services.AddSingleton(new DemoTokenIssuer(options.Demo, demoSigningKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearer =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = options.Mode == AuthMode.Firebase
                    ? FirebaseValidation(options.Firebase)
                    : DemoValidation(demoSigningKey);
                if (options.Mode == AuthMode.Firebase)
                {
                    bearer.Authority = options.Firebase.Issuer;
                }
            });

        services.AddAuthorization();
        return services;
    }

    private static TokenValidationParameters FirebaseValidation(FirebaseAuthOptions firebase) => new()
    {
        ValidIssuer = firebase.Issuer,
        ValidAudience = firebase.ProjectId,
        NameClaimType = EmailClaim
    };

    private static TokenValidationParameters DemoValidation(SymmetricSecurityKey signingKey) => new()
    {
        ValidIssuer = DemoAuthOptions.Issuer,
        ValidAudience = DemoAuthOptions.Issuer,
        IssuerSigningKey = signingKey,
        NameClaimType = EmailClaim
    };

    private static SymmetricSecurityKey DemoSigningKey(DemoAuthOptions demo) => new(
        string.IsNullOrWhiteSpace(demo.SigningKey)
            ? RandomNumberGenerator.GetBytes(32)
            : SHA256.HashData(Encoding.UTF8.GetBytes(demo.SigningKey)));
}

public sealed class DemoTokenIssuer(DemoAuthOptions demo, SymmetricSecurityKey signingKey)
{
    public bool Accepts(string email, string password) =>
        string.Equals(email.Trim(), demo.Email, StringComparison.OrdinalIgnoreCase) && password == demo.Password;

    public string Issue(TimeProvider clock) => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Issuer = DemoAuthOptions.Issuer,
        Audience = DemoAuthOptions.Issuer,
        Subject = new ClaimsIdentity([new Claim("email", demo.Email), new Claim("name", demo.Name)]),
        Expires = clock.GetUtcNow().Add(demo.SessionLifetime).UtcDateTime,
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
    });
}
