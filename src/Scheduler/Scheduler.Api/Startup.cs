using Assistant.Net.Messaging;
using Assistant.Net.Scheduler.Api.Conventions;
using Assistant.Net.Scheduler.Api.Handlers;
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
                    .UseMongo(Configuration.GetConnectionString("StorageDatabase"))
                    .AddMongo<Guid, AutomationModel>()
                    .AddMongo<Guid, JobModel>())
                .AddRemoteWebMessageHandler(b => b.AddConfiguration<LocalCommandHandlersConfiguration>());

            // todo: remove the line once the fix is released.
            //services.ReplaceSingleton<MessageExceptionJsonConverter>();

            // todo: configure logging, enrich request details, correlation id, machine name, thread...
            //services.AddLogging();

            // todo: implement authorization
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
                // todo: implement authorization for swagger
                //o.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme { });
                //o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { });
                //o.AddSecurityRequirement(new OpenApiSecurityRequirement { });
            });

            services.AddTransient<CorrelationMiddleware>();
            services.AddTransient<DiagnosticMiddleware>();
            services.AddTransient<ExceptionHandlingMiddleware>();
        }

        /// <summary/>
        public void Configure(IApplicationBuilder app) => app
            .UsePathBase("/api")
            .UseRouting()
            // todo: implement authorization
            //.UseAuthentication()
            //.UseAuthorization()
            .UseMiddleware<CorrelationMiddleware>()
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // todo: implement authorization
                //.RequireAuthorization()
            })
            .UseSwagger()
            .UseSwaggerUI()
            .UseMiddleware<DiagnosticMiddleware>()
            .UseRemoteWebMessageHandler();
    }
}
