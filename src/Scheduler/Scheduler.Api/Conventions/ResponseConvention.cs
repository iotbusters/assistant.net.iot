using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;

namespace Assistant.Net.Scheduler.Api.Conventions;

internal class ResponseConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        var method = action.Attributes.OfType<HttpMethodAttribute>().SingleOrDefault()?.HttpMethods.SingleOrDefault();
        if(method == null) return;

        switch (method)
        {
            case "GET":
                //var type = action.ActionMethod.ReturnType without Task<> wrapper;
                action.Filters.Add(new ProducesResponseTypeAttribute(/*type,*/StatusCodes.Status200OK));
                break;

            case "POST":
                action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status201Created));
                break;

            case "PUT":
                action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent));
                break;

            case "DELETE":
                action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent));
                break;
        }

        if (method != "GET")
            action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status400BadRequest));

        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status401Unauthorized));
        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status403Forbidden));

        if (method != "POST")
            action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status404NotFound));

        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType));
        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status500InternalServerError));

        action.Filters.Add(new ResponseMappingFilter());
    }
}