using Assistant.Net.Diagnostics.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Middlewares;

internal sealed class DiagnosticMiddleware : IMiddleware
{
    private readonly IDiagnosticFactory diagnosticFactory;

    public DiagnosticMiddleware(IDiagnosticFactory diagnosticFactory) =>
        this.diagnosticFactory = diagnosticFactory;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // only an /api calls require diagnostic, ignoring swagger endpoints.
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        var operation = diagnosticFactory.Start("scheduler-api-request");

        try
        {
            await next(context);

            if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
                operation.Fail("Handled internal error");
            else
                operation.Complete();
        }
        catch
        {
            operation.Fail("Unhandled exception");
            throw;
        }
    }
}