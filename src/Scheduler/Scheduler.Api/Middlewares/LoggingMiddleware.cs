using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Middlewares;

internal sealed class LoggingMiddleware : IMiddleware
{
    private readonly ILogger logger;

    public LoggingMiddleware(ILoggerFactory factory) =>
        logger = factory.CreateLogger(typeof(Startup).Namespace!);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // only an /api calls require logging, ignoring swagger endpoints.
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        var request = new
        {
            context.Request.Method,
            Url = context.Request.GetDisplayUrl(),
            CorrelationId = context.Request.Headers.GetCorrelationId(),
            context.Request.ContentLength
        };
        logger.LogInformation("API request: {@request}", request);

        try
        {
            await next(context);
        }
        finally
        {
            var response = new
            {
                CorrelationId = context.Response.Headers.GetCorrelationId(),
                context.Response.StatusCode,
                context.Response.ContentLength
            };
            logger.LogInformation("API response: {@response}", response);
        }
    }
}
