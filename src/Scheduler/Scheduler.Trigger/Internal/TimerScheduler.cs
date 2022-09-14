using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Internal;

internal sealed class TimerScheduler : ITimerScheduler
{
    private readonly ILogger<TimerScheduler> logger;
    private readonly ISystemClock clock;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, TriggerTimerModel> storage;
    private readonly ConcurrentDictionary<Guid, Task> scheduledTasks = new();

    public TimerScheduler(
        ILogger<TimerScheduler> logger,
        ISystemClock clock,
        IMessagingClient client,
        IAdminStorage<Guid, TriggerTimerModel> storage)
    {
        this.logger = logger;
        this.clock = clock;
        this.client = client;
        this.storage = storage;
    }

    public async Task RescheduleTimers(CancellationToken token)
    {
        logger.LogDebug("Query timers: begins.");

        var runIds = await storage.GetKeys(token).ToArrayAsync(token);

        if (!runIds.Any())
        {
            logger.LogDebug("Query timers: not found.");
            return;
        }

        logger.LogInformation("Query timers: found {TimerCount} arranged timer(s).", runIds.Length);

        foreach (var runId in runIds)
            await ScheduleTimer(runId, token);

        logger.LogInformation("Query timers: ends.");
    }

    public async Task ScheduleTimer(Guid runId, CancellationToken token)
    {
        using var runScope = logger.BeginPropertyScope("RunId", runId);

        var run = await client.Request(new RunQuery(runId), token);
        if (run.Status != RunStatus.Started)
        {
            logger.LogDebug("Schedule timer: run wasn't started.");
            return;
        }

        if (run.JobSnapshot.Configuration is not JobTimerConfigurationDto configuration)
        {
            logger.LogDebug("Schedule timer: not a timer.");
            return;
        }

        logger.LogDebug("Schedule timer: begins.");

        using var rollbackCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        var task = Schedule(runId, configuration, rollbackCancellationSource.Token)
            .ContinueWith(x =>
            {
                if (x.IsFaulted)
                    logger.LogCritical("");
                return scheduledTasks.TryRemove(runId, out _);
            }, CancellationToken.None);

        if (!scheduledTasks.TryAdd(runId, task))
        {
            rollbackCancellationSource.Cancel();
            logger.LogWarning("Schedule timer: already scheduled.");
        }

        logger.LogInformation("Schedule timer: ends.");
    }

    private async Task Schedule(Guid runId, JobTimerConfigurationDto configuration, CancellationToken token)
    {
        var timer = await storage.AddOrUpdate(
            key: runId,
            addFactory: _ => new(runId, configuration.NextTriggerTime(clock.UtcNow)),
            updateFactory: (_, last) =>
            {
                logger.LogWarning("Schedule timer: already arranged at {ArrangedTime:s}.", last.Arranged);
                return last;
            },
            token);

        var scheduled = clock.UtcNow;
        var delayTime = timer.Arranged - scheduled;
        if (delayTime > TimeSpan.Zero)
        {
            logger.LogInformation("Schedule timer: after {DelayTime}.", delayTime);

            await Task.Delay(delayTime, token);

            logger.LogInformation("Trigger timer: begins.");
        }
        else
            logger.LogWarning("Trigger timer: begins late.");

        var triggered = clock.UtcNow;
        await client.Publish(new TimerTriggeredEvent(timer.RunId, timer.Arranged, scheduled, triggered), token);

        await storage.TryRemove(timer.RunId, token);

        logger.LogInformation("Trigger timer: ends.");
    }
}
