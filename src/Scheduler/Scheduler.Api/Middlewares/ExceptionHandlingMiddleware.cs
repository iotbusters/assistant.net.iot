using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Scheduler.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Middlewares
{
    internal class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> logger;
        private readonly IDiagnosticFactory diagnosticFactory;

        public ExceptionHandlingMiddleware(
            ILogger<ExceptionHandlingMiddleware> logger,
            IDiagnosticFactory diagnosticFactory)
        {
            this.logger = logger;
            this.diagnosticFactory = diagnosticFactory;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var operation = diagnosticFactory.Start("exception-handling-middleware");
            try
            {
                await next(context);
                operation.Complete();
            }
            catch (NotFoundException)
            {
                operation.Complete();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Request has failed.");
                operation.Fail();
            }
            
        }
    }
}