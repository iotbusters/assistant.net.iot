﻿using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.EventHandler.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Assistant.Net.Scheduler.EventHandler;

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
        .AddGenericMessageHandling(b => b
            .UseMongo(ConfigureMessaging)
            //.AddHandler<TimerTriggeredEventHandler>()
            .AddHandler<RunSucceededEventHandler>()
            .AddHandler<RunFailedEventHandler>())
        .ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b
            .AddSingle<AutomationReferencesQuery>()
            .AddSingle<AutomationQuery>()
            .AddSingle<JobQuery>()
            .AddSingle<RunQuery>()
            .AddSingle<RunCreateCommand>()
            .AddSingle<RunStartCommand>()
            .AddSingle<RunSucceedCommand>()
            .AddSingle<RunDeleteCommand>());

    private void ConfigureMessaging(MongoOptions options) => options
        .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging))
        .Database(SchedulerMongoNames.DatabaseName);
}
