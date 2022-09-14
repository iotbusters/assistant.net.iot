using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Api.Conventions;

internal sealed class ResponseMappingFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var operation = context.ActionDescriptor.EndpointMetadata
            .OfType<HttpMethodAttribute>().SelectMany(x => x.HttpMethods).FirstOrDefault();
        if(operation == null) return;

        switch (operation)
        {
            case "GET" when context.Result is OkObjectResult:
                // todo: temporary. remove section after testing
                throw new InvalidOperationException("Default behavior. Remove GET case.");

            case "GET" when context.Result is ObjectResult result:
                context.Result = new OkObjectResult(result.Value);
                break;

            case "POST" when context.Result is ObjectResult {Value: Guid id}:
                var request = context.HttpContext.Request;
                var uriBuilder = new UriBuilder(
                    request.Scheme,
                    request.Host.Host,
                    request.Host.Port ?? -1, // default schema port
                    $"{request.PathBase}{request.Path}/{id:D}");
                context.Result = new CreatedResult(uriBuilder.Uri, null);
                break;

            case "PUT":
            case "DELETE":
                context.Result = new NoContentResult();
                break;
        }
    }
}