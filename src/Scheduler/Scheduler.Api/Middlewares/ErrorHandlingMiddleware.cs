using Assistant.Net.Abstractions;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Middlewares;

internal class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ISystemLifetime lifetime;
    private readonly ILogger logger;

    public ErrorHandlingMiddleware(
        ILogger<ErrorHandlingMiddleware> logger,
        ISystemLifetime lifetime)
    {
        this.logger = logger;
        this.lifetime = lifetime;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException e)
        {
            logger.LogDebug(e, "An exception has caught.");

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(ResourceNotFoundProblem, lifetime.Stopping);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception has caught.");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(RequestFailedProblem, lifetime.Stopping);
        }
    }

    private static ProblemDetails ResourceNotFoundProblem => new()
    {
        Title = "Resource wasn't found.", Detail = "Requested resource or one of its dependencies wasn't found."
    };

    private static ProblemDetails RequestFailedProblem => new()
    {
        Title = "Request has failed.", Detail = "Unexpected internal error has occurred."
    };
}
