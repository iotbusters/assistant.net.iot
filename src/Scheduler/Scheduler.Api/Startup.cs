using Assistant.Net.Messaging;
using Assistant.Net.Scheduler.Api.CommandHandlers;
using Assistant.Net.Scheduler.Api.Conventions;
using Assistant.Net.Scheduler.Api.Middlewares;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Assistant.Net.Scheduler.Api
{
    /// <summary>
    ///     Scheduler API startup configuration.
    /// </summary>
    public class Startup
    {
        /// <summary/>
        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        /// <summary/>
        public IConfiguration Configuration { get; }

        /// <summary/>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddStorage(b => b
                    .AddLocal<Guid, AutomationModel>()
                    .AddLocal<Guid, JobModel>())
                .AddMessagingClient(b => b.AddConfiguration<LocalCommandHandlersConfiguration>());

            services.AddLogging();

            // todo: implement authentication and authorization
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
                //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o => {});
                //.AddOAuth(JwtBearerDefaults.AuthenticationScheme, o => {});
            //services.AddAuthorization();
            services
                .AddControllers(options =>
                {
                    options.Conventions.Add(new ResponseConvention());
                    options.Conventions.Add(new ContentTypeConvention());
                    // todo: implement https://restfulapi.net/hateoas/
                    //options.Conventions.Add(new HateoasConvention());
                })
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var location = typeof(Startup).Assembly.Location;
            var folderPath = Path.GetDirectoryName(location)!;
            var fileName = Path.GetFileNameWithoutExtension(location);
            services.AddSwaggerGen(o =>
            {
                o.IncludeXmlComments(Path.Combine(folderPath, fileName + ".xml"), includeControllerXmlComments: true);
                // todo: implement https://restfulapi.net/hateoas/
                //o.DocumentFilter<HateoasDocumentFilter>();
                // todo: implement authentication and authorization for swagger
                //o.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme { });
                //o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { });
                //o.AddSecurityRequirement(new OpenApiSecurityRequirement { });
            });

            services.AddTransient<ExceptionHandlingMiddleware>();
        }

        /// <summary/>
        public void Configure(IApplicationBuilder app) => app
            .UsePathBase("/api")
            .UseRouting()
            // todo: implement authentication
            //.UseAuthentication()
            //.UseAuthorization()
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseEndpoints(endpoints => endpoints
                .MapControllers()
                //.RequireAuthorization()
            )
            .UseSwagger()
            .UseSwaggerUI();
    }
}
