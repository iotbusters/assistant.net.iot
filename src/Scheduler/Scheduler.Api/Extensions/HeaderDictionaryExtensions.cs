using Assistant.Net.Messaging;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Api.Extensions
{
    /// <summary>
    ///     Header dictionary extensions for web api.
    /// </summary>
    public static class HeaderDictionaryExtensions
    {
        /// <summary>
        ///     Gets a header value from the request/response and fail if it's missing.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static string GetRequiredHeader(this IHeaderDictionary headers, string name)
        {
            if (!headers.TryGetValue(name, out var values) || !values.Any())
                throw new InvalidOperationException($"Header '{name}' is required.");

            return values.First();
        }

        /// <summary>
        ///     Adds a header to the request/response.
        /// </summary>
        public static IHeaderDictionary TryAddHeader(this IHeaderDictionary headers, string name, Func<string> getCorrelationId)
        {
            if (!headers.ContainsKey(name))
                headers.Add(name, getCorrelationId());
            return headers;
        }

        /// <summary>
        ///     Gets a correlation id header value from the request/response and fail if it's missing.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static string GetCorrelationId(this IHeaderDictionary headers) => headers
            .GetRequiredHeader(HeaderNames.CorrelationId);

        /// <summary>
        ///     Adds a correlation id to the request/response headers.
        /// </summary>
        public static IHeaderDictionary TryAddCorrelationId(this IHeaderDictionary headers, Func<string> getCorrelationId) =>
            headers.TryAddHeader(HeaderNames.CorrelationId, getCorrelationId);
    }
}