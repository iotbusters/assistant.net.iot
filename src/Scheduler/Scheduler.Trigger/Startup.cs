using Assistant.Net.Abstractions;
using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Assistant.Net.Scheduler.Trigger.Handlers;
using Assistant.Net.Scheduler.Trigger.Internal;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        .AddHostedService<TimerTriggerService>()
        .AddSingleton<ITimerScheduler, TimerScheduler>()
        .AddSingleton<IEventTriggerService, EventTriggerService>()
        .AddMessagingClient(b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<RunQuery>()
            .AddSingle<TriggerQuery>()
            .AddSingle<TriggerReferencesQuery>())
        .AddMessagingClient(GenericOptionsNames.DefaultName, b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<RunSucceedCommand>()
            .AddSingle<TriggerQuery>())
        .AddStorage(b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<Guid, TriggerTimerModel>())
        .AddStorage(GenericOptionsNames.DefaultName, b => b
            .UseMongo(ConfigureMessaging)
            .UseMongoSingleProvider()
            .AddSingle<Guid, TriggerTimerModel>())
        .AddGenericMessageHandling(b => b
            .UseMongo(ConfigureMessaging)
            .AddHandler<TriggerEventHandlers>())
        .AddSingleton<ReloadableEventTriggerOptionsSource>()
        .BindOptions<EventTriggerOptions, IConfigureOptionsSource<EventTriggerOptions>>(GenericOptionsNames.DefaultName, p =>
            p.GetRequiredService<ReloadableEventTriggerOptionsSource>())
        .AddOptions<MessagingClientOptions>(GenericOptionsNames.DefaultName)
        .ChangeOn<EventTriggerOptions>(GenericOptionsNames.DefaultName, (mo, eo) =>
            mo.AddEventHandlerOf(eo.EventTriggers.Keys.ToArray()))
        .Services
        .AddOptions<GenericHandlingServerOptions>()
        .ChangeOn<EventTriggerOptions>(GenericOptionsNames.DefaultName, (so, eo) =>
            so.AcceptMessages(eo.EventTriggers.Keys.ToArray()));

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}
