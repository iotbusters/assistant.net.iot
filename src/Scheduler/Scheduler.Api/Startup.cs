using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Api.Conventions;
using Assistant.Net.Scheduler.Api.Handlers;
using Assistant.Net.Scheduler.Api.Middlewares;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Assistant.Net.Scheduler.Api;

/// <summary>
///     Scheduler API startup configuration.
/// </summary>
public class Startup
{
    /// <summary />
    public Startup(IConfiguration configuration) =>
        Configuration = configuration;

    /// <summary />
    public IConfiguration Configuration { get; }

    /// <summary />
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddStorage(GenericOptionsNames.DefaultName, b => b
                .UseMongo(ConfigureStorage)
                .UseMongoSingleProvider()
                .AddSingle<Guid, AutomationModel>()
                .AddSingle<Guid, JobModel>()
                .AddSingle<Guid, RunModel>()
                .AddSingle<Guid, TriggerModel>())
            .AddGenericMessageHandling(b => b
                .UseMongo(ConfigureMessaging)
                .AddHandler<AutomationHandlers>()
                .AddHandler<JobHandlers>()
                .AddHandler<RunHandlers>()
                .AddHandler<TriggerHandlers>())
            .ConfigureGenericMessagingClient(b => b
                .UseMongo(ConfigureMessaging)
                .UseMongoSingleProvider()
                .AddSingle<RunSucceededEvent>()
                .AddSingle<RunFailedEvent>());

        // todo: configure logging, enrich request details, correlation id, machine name, thread...
        //services.AddLogging(b => b.AddConsole());

        // todo: implement authorization
        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o => {});
        //.AddOAuth(JwtBearerDefaults.AuthenticationScheme, o => {});
        //services.AddAuthorization();
        services.AddControllers(o =>
        {
            o.Conventions.Add(new ResponseConvention());
            o.Conventions.Add(new ContentTypeConvention());
            // todo: implement https://restfulapi.net/hateoas/
            //options.Conventions.Add(new HateoasConvention());
        });
        services.Configure<JsonOptions, IServiceProvider>((o, p) =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.Converters.Add(new AdvancedJsonConverterFactory(p));
        });

        var location = typeof(Startup).Assembly.Location;
        var folderPath = Path.GetDirectoryName(location)!;
        var fileName = Path.GetFileNameWithoutExtension(location);
        services.AddSwaggerGen(o =>
        {
            o.UseOneOfForPolymorphism();
            o.IncludeXmlComments(Path.Combine(folderPath, fileName + ".xml"), true);
            // todo: implement https://restfulapi.net/hateoas/
            //o.DocumentFilter<HateoasDocumentFilter>();
            // todo: implement authorization for swagger
            //o.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme { });
            //o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { });
            //o.AddSecurityRequirement(new OpenApiSecurityRequirement { });
        });

        services.AddTransient<CorrelationMiddleware>();
        services.AddTransient<LoggingMiddleware>();
        services.AddTransient<DiagnosticMiddleware>();
        services.AddTransient<ErrorHandlingMiddleware>();
    }

    /// <summary />
    public void Configure(IApplicationBuilder app)
    {
        const string baseUrl = "api";
        app
            .UsePathBase($"/{baseUrl}")
            .UseRouting()
            // todo: implement authorization
            //.UseAuthentication()
            //.UseAuthorization()
            .UseMiddleware<CorrelationMiddleware>()
            .UseMiddleware<LoggingMiddleware>()
            .UseMiddleware<ErrorHandlingMiddleware>()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // todo: implement authorization
                //.RequireAuthorization()
            })
            .UseSwagger(o => o.PreSerializeFilters.Add((doc, req) => doc.Servers.Add(
                new OpenApiServer {Url = $"{req.Scheme}://{req.Host}/{baseUrl}"})))
            .UseSwaggerUI()
            .UseMiddleware<DiagnosticMiddleware>();
    }

    private void ConfigureStorage(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Database))
        .Database(SchedulerMongoNames.DatabaseName);

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}
