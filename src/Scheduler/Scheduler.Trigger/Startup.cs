using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Internal;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        .Configure<TimerOptions>(Configuration.GetSection(ConfigurationNames.TriggerTimer))
        .AddHostedService<TriggerPollingService>()
        .AddHostedService<TimerTriggerService>()
        .AddMessagingClient(b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<TimerTriggeredEvent>()
            .AddSingle<RunQuery>()
            .AddSingle<TriggerQuery>()
            .AddSingle<TriggerReferencesQuery>())
        .AddStorage(b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<Guid, TriggerTimerModel>())
        .AddGenericMessageHandling(b => b.UseMongo(ConfigureMessaging))
        // avoid intersecting with other handler's caching results.
        .ConfigureGenericMessagingClient(b => b
            .RemoveInterceptor<CachingInterceptor>()
            .AddSingle<TriggerQuery>()
            .AddSingle<RunUpdateCommand>())
        // configure a placeholder to pass validation
        .ConfigureGenericHandlingServerOptions(o => o.MessageTypes.Add(typeof(IAbstractMessage)))
        .BindOptions<MessagingClientOptions, ReloadableMessagingClientOptionsSource>(GenericOptionsNames.DefaultName);

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}
