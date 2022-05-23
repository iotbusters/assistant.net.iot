using Assistant.Net.Messaging;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.EventHandler.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Scheduler.EventHandler;

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
        .AddMongoMessageHandling(b => b
            .UseMongo(ConfigureMessaging)
            .AddHandler<TimerTriggeredEventHandler>()
            .AddHandler<RunSucceededEventHandler>()
            .AddHandler<RunFailedEventHandler>())
        .AddMessagingClient(b => b
            .AddHandler<TimerTriggeredEventHandler>()
            .AddHandler<RunSucceededEventHandler>()
            .AddHandler<RunFailedEventHandler>()
            .UseMongo(ConfigureMessaging)
            .AddMongo<AutomationReferencesQuery>()
            .AddMongo<AutomationQuery>()
            .AddMongo<JobQuery>()
            .AddMongo<RunQuery>()
            .AddMongo<RunCreateCommand>()
            .AddMongo<RunUpdateCommand>()
            .AddMongo<RunDeleteCommand>());

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}