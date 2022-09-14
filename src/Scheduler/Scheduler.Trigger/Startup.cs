using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Assistant.Net.Scheduler.Trigger.Handlers;
using Assistant.Net.Scheduler.Trigger.Internal;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;

namespace Assistant.Net.Scheduler.Trigger;

/// <summary/>
public sealed class Startup
{
    /// <summary/>
    public Startup(IConfiguration configuration) =>
        Configuration = configuration;

    /// <summary/>
    public IConfiguration Configuration { get; }

    /// <summary/>
    public void ConfigureServices(IServiceCollection services) => services
        .AddLogging(b => b
            .AddYamlConsole()
            .AddPropertyScope("ApplicationName", p => p.GetRequiredService<IHostEnvironment>().ApplicationName)
            .AddPropertyScope("Thread", () => Thread.CurrentThread.ManagedThreadId))
        .Configure<TriggerPollingOptions>(Configuration.GetSection(ConfigurationNames.TriggerPolling))
        .Configure<TimerTriggerOptions>(Configuration.GetSection(ConfigurationNames.TriggerTimer))
        //.AddHostedService<TriggerPollingService>()
        .AddHostedService<TimerTriggerService>()
        .AddSingleton<ITimerScheduler, TimerScheduler>()
        .AddSingleton<IEventTriggerService, EventTriggerService>()
        .AddMessagingClient(b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<RunQuery>()
            .AddSingle<TriggerQuery>()
            .AddSingle<TriggerReferencesQuery>()
            .AddSingle<RunSucceedCommand>())
        .ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            // avoid intersecting with other handler's caching results.
            .RemoveInterceptor<CachingInterceptor>()
            .AddSingle<TriggerQuery>())
        .AddGenericMessageHandling(b => b.UseMongo(ConfigureMessaging).AddHandler<TriggerCreatedEventHandler>())
        // configure a placeholder to pass validation
        //.ConfigureGenericHandlingServerOptions(o => o.MessageTypes.Add(typeof(IAbstractMessage)))
        .AddStorage(b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<Guid, TriggerTimerModel>())
        .BindOptions<EventTriggerOptions, ReloadableEventTriggerOptionsSource>()
        .AddOptions<MessagingClientOptions>()
        .ChangeOn<EventTriggerOptions>("", (mo, eo) => mo
            .AddEventHandlerOf(eo.EventTriggers.Keys.ToArray()));
        //.Services
        //.AddOptions<GenericHandlingServerOptions>()
        //.ChangeOn<EventTriggerOptions>(GenericOptionsNames.DefaultName, (mo, eo) =>
        //{
        //    foreach (var messageType in eo.EventTriggers.Keys)
        //        mo.MessageTypes.Add(messageType);
        //});

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}
