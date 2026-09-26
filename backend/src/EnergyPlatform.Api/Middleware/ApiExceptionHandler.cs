using EnergyPlatform.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnergyPlatform.Api.Middleware;

internal sealed class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            DomainRuleException ruleViolation => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "La operación no está permitida",
                Detail = ruleViolation.Message
            },
            BadHttpRequestException badRequest => new ProblemDetails
            {
                Status = badRequest.StatusCode,
                Title = "La petición no es válida",
                Detail = "Revisa el formato del JSON y que los valores sean de los permitidos."
            },
            _ => null
        };

        if (problem is null)
        {
            return false;
        }

        logger.LogWarning("Petición rechazada en {Path}: {Message}", httpContext.Request.Path, exception.Message);
        problem.Instance = httpContext.Request.Path;
        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem });
    }
}
