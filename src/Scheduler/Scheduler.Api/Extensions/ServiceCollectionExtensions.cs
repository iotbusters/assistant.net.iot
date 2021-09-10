using Assistant.Net.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Scheduler.Api.Extensions
{
    /// <summary>
    ///     Service collection extensions for web api.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers diagnostic context based on <see cref="IHttpContextAccessor"/>.
        /// </summary>
        public static IServiceCollection AddDiagnosticContextWebHosted(this IServiceCollection services) => services
            .AddHttpContextAccessor()
            .AddDiagnosticContext(InitializeFromHttpContext);

        private static string InitializeFromHttpContext(IServiceProvider provider)
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            var context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext wasn't yet initialized.");

            return context.Request.Headers.GetCorrelationId();
        }
    }
}