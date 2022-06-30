using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Internal;

internal class TimerTriggerService : BackgroundService
{
    private readonly ILogger<TimerTriggerService> logger;
    private readonly IOptionsMonitor<TimerOptions> options;
    private readonly ISystemClock clock;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, TriggerTimerModel> storage;

    public TimerTriggerService(
        ILogger<TimerTriggerService> logger,
        IOptionsMonitor<TimerOptions> options,
        ISystemClock clock,
        IMessagingClient client,
        IAdminStorage<Guid, TriggerTimerModel> storage)
    {
        this.logger = logger;
        this.options = options;
        this.clock = clock;
        this.client = client;
        this.storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var gracefulShutdownSource = new CancellationTokenSource();
        await using var _ = token.Register(() => gracefulShutdownSource.CancelAfter(options.CurrentValue.CancellationDelay));

        logger.LogInformation("Start scheduling triggers.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                await FindAndSchedule(gracefulShutdownSource.Token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                continue;
            }

            await Task.WhenAny(Task.Delay(options.CurrentValue.InactivityDelayTime, token));
        }

        logger.LogInformation("Stop scheduling triggers.");
    }

    private async Task FindAndSchedule(CancellationToken token)
    {
        logger.LogDebug("Query available triggers.");

        var timerQuery = await storage.GetKeys(token)
            .Select(async x => await storage.GetOrDefault(x, token))
            .Where(x => x != default)
            .Select(x => x!)
            .AsEnumerableAsync();
        var allTimers = timerQuery.ToArray();
        var triggeredTimers = allTimers.Where(x => x.IsTriggered).ToArray();
        var arrangedTimers = allTimers.Where(x => !x.IsTriggered).ToArray();

        logger.LogInformation("Found timers {ArrangedCount}/{TriggeredCount}.", arrangedTimers.Length, triggeredTimers.Length);
        await CleanupTriggeredTimers(triggeredTimers, token);

        if (!arrangedTimers.Any())
        {
            logger.LogDebug("Found no arranged timers.");
            return;
        }

        logger.LogInformation("Schedule {ArrangedCount} timers.", arrangedTimers.Length);
        foreach (var timer in arrangedTimers.OrderBy(x => x.Arranged))
            await ScheduleTimer(timer, token);
    }

    private async Task CleanupTriggeredTimers(IEnumerable<TriggerTimerModel> triggeredTimers, CancellationToken token)
    {
        var serverOptions = options.CurrentValue;
        var expiredTimers = triggeredTimers
            .Where(x => clock.UtcNow - x.Arranged > serverOptions.TriggeredTimerExpirationTime)
            .ToArray();

        if (!expiredTimers.Any())
        {
            logger.LogDebug("Found no triggered timers.");
            return;
        }

        foreach (var timer in expiredTimers)
            await storage.TryRemove(timer.RunId, token);

        logger.LogInformation("Removed timers {ExpiredCount}.", expiredTimers.Length);
    }

    private async Task ScheduleTimer(TriggerTimerModel timer, CancellationToken token)
    {
        var scheduled = clock.UtcNow;
        var delayTime = timer.Arranged - scheduled;
        if (delayTime > TimeSpan.Zero)
        {
            logger.LogInformation("Timer({RunId})[{ArrangedTime}] trigger: scheduled after {DelayTime}.",
                timer.RunId, timer.Arranged, delayTime);

            await Task.Delay(delayTime, token);

            logger.LogInformation("Timer({RunId})[{ArrangedTime}] trigger: begins.", timer.RunId, timer.Arranged);
        }
        else
            logger.LogWarning("Timer({RunId})[{ArrangedTime}] trigger: begins late.", timer.RunId, timer.Arranged);

        await client.Publish(new TimerTriggeredEvent(
            timer.RunId,
            arranged: timer.Arranged,
            scheduled: scheduled,
            triggered: clock.UtcNow), token);

        try
        {
            await storage.AddOrUpdate(
                timer.RunId,
                addFactory: _ => throw new StorageException($"Timer({timer.RunId}) doesn't exist."),
                updateFactory: (_, current) =>
                {
                    if (current.RunId != timer.RunId || current.IsTriggered)
                        throw new StorageException($"Timer({timer.RunId}) was already switched to another Timer({current.RunId}).");
                    return current with {IsTriggered = true};
                },
                token);
        }
        catch (StorageException ex)
        {
            logger.LogCritical(ex, "Timer({RunId}) trigger: failed.", timer.RunId);
            return;
        }

        logger.LogInformation("Timer({RunId}) trigger: succeeded.", timer.RunId);
    }
}
