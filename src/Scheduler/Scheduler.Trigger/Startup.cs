using Assistant.Net.Abstractions;
using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Internal;
using Assistant.Net.Scheduler.Trigger.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Scheduler.Trigger;

/// <summary/>
public class Startup
{
    /// <summary/>
    public Startup(IConfiguration configuration) =>
        Configuration = configuration;

    /// <summary/>
    public IConfiguration Configuration { get; }

    /// <summary/>
    public void ConfigureServices(IServiceCollection services) => services
        .Configure<TriggerPollingOptions>(Configuration.GetSection(ConfigurationNames.TriggerPolling))
        .AddHostedService<TriggerPollingService>()
        .AddMongoMessageHandling(b => b
            .UseMongo(ConfigureMessaging))
        .AddMessagingClient(b => b
            .UseMongo(ConfigureMessaging)
            .AddMongo<TriggerQuery>()
            .AddMongo<TriggerReferencesQuery>())
        .AddOptions<MessagingClientOptions>(MongoOptionsNames.DefaultName)
        .Bind<ReloadableOptionsSource>()
        .Services // the replace works around a bug.
        .ReplaceSingleton<IConfigureOptionsSource<MessagingClientOptions>>(p => p.GetRequiredService<ReloadableOptionsSource>())
        .AddSingleton<ReloadableOptionsSource>();

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}