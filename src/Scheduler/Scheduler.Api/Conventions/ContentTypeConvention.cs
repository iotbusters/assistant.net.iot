using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;
using System.Net.Mime;

namespace Assistant.Net.Scheduler.Api.Conventions
{
    internal class ContentTypeConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            var method = action.Attributes.OfType<HttpMethodAttribute>().SingleOrDefault()?.HttpMethods.SingleOrDefault();
            if (method == null) return;

            switch (method)
            {
                case "GET":
                    action.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
                    break;

                case "POST":
                    action.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
                    break;

                case "PUT":
                    action.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
                    break;
            }
        }
    }
}