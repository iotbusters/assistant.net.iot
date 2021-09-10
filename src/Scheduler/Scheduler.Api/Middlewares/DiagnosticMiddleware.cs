using Assistant.Net.Diagnostics.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Middlewares
{
    internal class DiagnosticMiddleware : IMiddleware
    {
        private readonly IDiagnosticFactory diagnosticFactory;

        public DiagnosticMiddleware(IDiagnosticFactory diagnosticFactory) =>
            this.diagnosticFactory = diagnosticFactory;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var operation = diagnosticFactory.Start("diagnistic-middleware");

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
}