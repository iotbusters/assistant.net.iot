using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Internal;

internal class TriggerPollingService : BackgroundService
{
    private readonly HashSet<Guid> unsupportedRunIds = new();
    private Dictionary<Guid, Type> events = new();

    private readonly ILogger logger;
    private readonly IOptionsMonitor<TriggerPollingOptions> options;
    private readonly ReloadableMessagingClientOptionsSource eventSource;
    private readonly IStorage<Guid, TriggerTimerModel> storage;
    private readonly IMessagingClient client;
    private readonly ISystemClock clock;
    private readonly ITypeEncoder typeEncoder;

    private readonly CancellationTokenSource gracefulShutdownSource = new();
    private CancellationTokenRegistration registration;

    public TriggerPollingService(
        ILogger<TriggerPollingService> logger,
        IOptionsMonitor<TriggerPollingOptions> options,
        ReloadableMessagingClientOptionsSource eventSource,
        IStorage<Guid, TriggerTimerModel> storage,
        ITypeEncoder typeEncoder,
        IMessagingClient client,
        ISystemClock clock)
    {
        this.logger = logger;
        this.options = options;
        this.eventSource = eventSource;
        this.storage = storage;
        this.typeEncoder = typeEncoder;
        this.client = client;
        this.clock = clock;
    }

    public override Task StartAsync(CancellationToken token)
    {
        registration = token.Register(() => gracefulShutdownSource.CancelAfter(options.CurrentValue.CancellationDelay));
        return base.StartAsync(token);
    }

    public override void Dispose()
    {
        registration.Dispose();
        gracefulShutdownSource.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        logger.LogInformation("Start polling triggers resource.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                await ReloadAsync(gracefulShutdownSource.Token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                continue;
            }

            await Task.WhenAny(Task.Delay(options.CurrentValue.InactivityDelayTime, token));
        }

        logger.LogInformation("Stop polling triggers resource.");
    }

    public async Task ReloadAsync(CancellationToken token)
    {
        logger.LogDebug("Reloading triggers to handle.");

        var references = await client.Request(new TriggerReferencesQuery(), token);
        var latestRunIds = references.Select(x => x.RunId).Except(unsupportedRunIds).ToHashSet();

        var previousEventTypes = events;
        var previousRunIds = previousEventTypes.Keys;
        if (latestRunIds.SequenceEqual(previousRunIds))
        {
            logger.LogDebug("No new triggers were found.");
            return;
        }

        var supportedEventTypes = await GetSupportedEventTypes(latestRunIds, previousRunIds, token);
        if (!supportedEventTypes.Any())
            logger.LogWarning("No new trigger events were found.");

        events = (
                from id in latestRunIds
                join pet in previousEventTypes on id equals pet.Key into pets
                join set in supportedEventTypes on id equals set.Key into sets
                let type = pets.FirstOrDefault().Value ?? sets.FirstOrDefault().Value
                where type != null
                select (id, type))
            .ToDictionary(x => x.id, x => x.type);
        eventSource.Reload(events);

        logger.LogInformation("Reloaded supported trigger events {TotalEventCount} (+{NewEventCount}/-{OldEventCount}) to handle.",
            events.Count, supportedEventTypes.Count, Math.Max(0, previousEventTypes.Count - events.Count));

        var timerRunIds = supportedEventTypes.Where(x => x.Value == typeof(TimerTriggeredEvent)).Select(x => x.Key);
        await ScheduleTimers(timerRunIds, token);
    }

    private async Task<Dictionary<Guid, Type>> GetSupportedEventTypes(
        IEnumerable<Guid> latestRunIds,
        IEnumerable<Guid> previousRunIds,
        CancellationToken token)
    {
        var triggerQueryTasks = latestRunIds
            .Except(previousRunIds)
            .Select(x => client.Request(new TriggerQuery(x), token));
        var triggers = await Task.WhenAll(triggerQueryTasks);

        var triggerConfigurations = triggers
            .Select(x => (id: x.RunId, name: x.TriggerEventName, type: typeEncoder.Decode(x.TriggerEventName)))
            .ToArray();

        var unsupportedConfigurations = triggerConfigurations.Where(x => x.type == null).ToArray();
        foreach (var @event in unsupportedConfigurations)
        {
            unsupportedRunIds.Add(@event.id);
            logger.LogError("Unknown {EventName} ({RunId}) is ignored.", @event.name, @event.id);
        }

        return triggerConfigurations.Where(x => x.type != null).ToDictionary(x => x.id, x => x.type!);
    }

    private async Task ScheduleTimers(IEnumerable<Guid> timerRunIds, CancellationToken token)
    {
        var runQueryTasks = timerRunIds.Select(x => client.Request(new RunQuery(x), token));
        var runs = await Task.WhenAll(runQueryTasks);

        var timers = runs
            .Where(x => x.Status.Value == RunStatus.Started)
            .Where(x => x.JobSnapshot.Configuration is JobTimerConfigurationDto)
            .Select(x => (RunId: x.Id, JobId: x.JobSnapshot.Id, Configuration: (JobTimerConfigurationDto) x.JobSnapshot.Configuration))
            .ToArray();

        foreach (var timer in timers)
        {
            if (await storage.TryGet(timer.JobId, token) is not Some<TriggerTimerModel>({IsTriggered : false}))
                continue;

            await storage.AddOrUpdate(
                key: timer.JobId,
                addFactory: _ => new(timer.RunId, timer.Configuration.NextTriggerTime(clock.UtcNow), IsTriggered: false),
                updateFactory: (_, last) => new(timer.RunId, timer.Configuration.NextTriggerTime(last.Arranged), IsTriggered: false),
                token);
        }

        if (timers.Any())
            logger.LogInformation("Configured {NewTimerCount} to trigger.", timers.Length);
        else
            logger.LogDebug("No new trigger timers were found.");
    }
}
