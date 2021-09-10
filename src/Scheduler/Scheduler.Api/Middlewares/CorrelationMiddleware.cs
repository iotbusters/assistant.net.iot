using Assistant.Net.Scheduler.Api.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Middlewares
{
    internal class CorrelationMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Request.Headers.TryAddCorrelationId(() => Guid.NewGuid().ToString());

            // only an /api calls require correlation id in response, ignoring swagger endpoints.
            if (context.Request.Path.StartsWithSegments("/api"))
                context.Response.Headers.TryAddCorrelationId(() => context.Request.Headers.GetCorrelationId());

            return next(context);
        }
    }
}