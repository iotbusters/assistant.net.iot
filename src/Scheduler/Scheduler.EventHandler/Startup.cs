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
        .AddGenericMessageHandling(b => b
            .UseMongo(ConfigureMessaging)
            .AddHandler<TimerTriggeredEventHandler>()
            .AddHandler<RunSucceededEventHandler>()
            .AddHandler<RunFailedEventHandler>())
        .ConfigureGenericMessagingClient(b=> b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<AutomationReferencesQuery>()
            .AddSingle<AutomationQuery>()
            .AddSingle<JobQuery>()
            .AddSingle<RunQuery>()
            .AddSingle<RunCreateCommand>()
            .AddSingle<RunUpdateCommand>()
            .AddSingle<RunDeleteCommand>());

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}
