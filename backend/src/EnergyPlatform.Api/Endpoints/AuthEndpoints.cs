using EnergyPlatform.Api.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EnergyPlatform.Api.Endpoints;

public static class AuthEndpoints
{
    public sealed record FirebaseWebConfig(string ApiKey, string AuthDomain, string ProjectId, string AppId);

    public sealed record DemoHint(string Email, string Password);

    public sealed record ClientConfigResponse(AuthMode AuthMode, FirebaseWebConfig? Firebase, DemoHint DemoAccount);

    public sealed record LoginRequest(string Email, string Password);

    public sealed record LoginResponse(string Token, string Name, string Email);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/config", (AuthOptions options) => TypedResults.Ok(new ClientConfigResponse(
                options.Mode,
                options.Mode == AuthMode.Firebase
                    ? new FirebaseWebConfig(options.Firebase.ApiKey, options.Firebase.AuthDomain, options.Firebase.ProjectId, options.Firebase.AppId)
                    : null,
                new DemoHint(options.Demo.Email, options.Demo.Password))))
            .WithName("GetClientConfig")
            .WithTags("Autenticación")
            .WithSummary("Modo de autenticación (Firebase o demo), configuración pública de Firebase y la cuenta de demo");

        app.MapPost("/api/auth/login", Results<Ok<LoginResponse>, UnauthorizedHttpResult, NotFound> (
                LoginRequest request,
                AuthOptions options,
                DemoTokenIssuer issuer,
                TimeProvider clock) =>
            {
                if (options.Mode != AuthMode.Demo)
                {
                    return TypedResults.NotFound();
                }

                return issuer.Accepts(request.Email, request.Password)
                    ? TypedResults.Ok(new LoginResponse(issuer.Issue(clock), options.Demo.Name, options.Demo.Email))
                    : TypedResults.Unauthorized();
            })
            .WithName("Login")
            .WithTags("Autenticación")
            .WithSummary("Inicia sesión en modo demo y devuelve un token")
            .WithDescription("Solo disponible con Auth:Mode = Demo. En modo Firebase el inicio de sesión lo hace el SDK de Firebase en el navegador.");
    }
}
