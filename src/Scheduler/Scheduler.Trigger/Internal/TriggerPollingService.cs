//using Assistant.Net.Abstractions;
//using Assistant.Net.Messaging.Abstractions;
//using Assistant.Net.Messaging.HealthChecks;
//using Assistant.Net.Scheduler.Contracts.Events;
//using Assistant.Net.Scheduler.Contracts.Queries;
//using Assistant.Net.Scheduler.Trigger.Abstractions;
//using Assistant.Net.Scheduler.Trigger.Options;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Assistant.Net.Scheduler.Trigger.Internal;

//internal sealed class TriggerPollingService : BackgroundService
//{
//    private readonly HashSet<Guid> unsupportedRunIds = new();
//    private Dictionary<Type, IList<Guid>> eventTriggers = new();

//    private readonly ILogger logger;
//    private readonly IOptionsMonitor<TriggerPollingOptions> options;
//    private readonly ReloadableEventTriggerOptionsSource eventTriggerOptionsSource;
//    private readonly ITypeEncoder typeEncoder;
//    private readonly ISystemClock clock;
//    private readonly ISystemLifetime lifetime;
//    private readonly IMessagingClient client;
//    private readonly ITimerScheduler scheduler;
//    private readonly ServerActivityService activityService;

//    public TriggerPollingService(
//        ILogger<TriggerPollingService> logger,
//        IOptionsMonitor<TriggerPollingOptions> options,
//        ReloadableEventTriggerOptionsSource eventTriggerOptionsSource,
//        ITypeEncoder typeEncoder,
//        ISystemClock clock,
//        ISystemLifetime lifetime,
//        IMessagingClient client,
//        ITimerScheduler scheduler,
//        ServerActivityService activityService)
//    {
//        this.logger = logger;
//        this.options = options;
//        this.eventTriggerOptionsSource = eventTriggerOptionsSource;
//        this.typeEncoder = typeEncoder;
//        this.clock = clock;
//        this.lifetime = lifetime;
//        this.client = client;
//        this.scheduler = scheduler;
//        this.activityService = activityService;
//    }

//    protected override async Task ExecuteAsync(CancellationToken token)
//    {
//        logger.LogInformation("Polling triggers: begins.");

//        var stoppingToken = lifetime.Stopping;
//        var serverOptions = options.CurrentValue;
//        var strategy = serverOptions.Retry;
//        var attempt = 1;

//        while (!stoppingToken.IsCancellationRequested)
//        {
//            await activityService.DelayInactive(token);

//            try
//            {
//                await ReloadAsync(token);
//            }
//            catch (OperationCanceledException ex) when (token.IsCancellationRequested)
//            {
//                logger.LogWarning(ex, "Polling triggers: cancelled.");
//                break;
//            }
//            catch(Exception ex)
//            {
//                attempt++;
//                if (!strategy.CanRetry(attempt))
//                {
//                    logger.LogCritical(ex, "Polling triggers: failed.");
//                    break;
//                }

//                logger.LogError(ex, "Polling triggers: #{Attempt} failed.", attempt);

//                await Task.WhenAny(Task.Delay(strategy.DelayTime(attempt), token));
//                continue;
//            }

//            attempt = 1;
//            await Task.WhenAny(Task.Delay(serverOptions.InactivityDelayTime, token));
//        }

//        logger.LogInformation("Polling triggers: ends.");
//    }

//    public async Task ReloadAsync(CancellationToken token)
//    {
//        logger.LogDebug("Reloading triggers: begins.");

//        var references = await client.Request(new TriggerReferencesQuery(), token);
//        var latestRunIds = references.Select(x => x.RunId).Except(unsupportedRunIds).ToHashSet();

//        var previousEventTypes = eventTriggers.SelectMany(x => x.Value.Select(id => (type: x.Key, id))).ToArray();
//        var previousRunIds = previousEventTypes.Select(x => x.id).ToHashSet();
//        if (latestRunIds.SequenceEqual(previousRunIds))
//        {
//            logger.LogDebug("No new triggers were found.");
//            return;
//        }

//        var newRunIds = latestRunIds.Except(previousRunIds);
//        var supportedEventTriggers = await GetEventTriggers(newRunIds, token);
//        if (!supportedEventTriggers.Any())
//            logger.LogWarning("No new trigger events were found.");

//        eventTriggers = (
//                from id in latestRunIds
//                join pet in previousEventTypes on id equals pet.id into pets
//                join set in supportedEventTriggers on id equals set.id into sets
//                from e in pets.Concat(sets)
//                select e)
//            .GroupBy(x => x.type, x => x.id).ToDictionary(x => x.Key, x => (IList<Guid>)x.ToList());
//        eventTriggerOptionsSource.Reload(eventTriggers);

//        if (eventTriggers.Any())
//            logger.LogInformation("Reloading triggers: configured {TotalCount} ({NewCount}/{OldCount}).",
//                eventTriggers.Count, supportedEventTriggers.Length, Math.Max(0, previousEventTypes.Length - eventTriggers.Count));
//        else
//            logger.LogDebug("Reloading triggers: not found.");

//        var timerRunIds = supportedEventTriggers.Where(x => x.type == typeof(TimerTriggeredEvent)).Select(x => x.id).Distinct();
//        foreach (var runId in timerRunIds)
//            await scheduler.ScheduleTimer(runId, token);

//        logger.LogInformation("Reloading triggers: ends.");
//    }

//    private async Task<(Type type, Guid id)[]> GetEventTriggers(IEnumerable<Guid> runIds, CancellationToken token)
//    {
//        var triggerQueryTasks = runIds.Select(x => client.Request(new TriggerQuery(x), token));
//        var triggers = await Task.WhenAll(triggerQueryTasks);

//        var triggerConfigurations = triggers
//            .Select(x => (id: x.RunId, name: x.TriggerEventName, type: typeEncoder.Decode(x.TriggerEventName)))
//            .ToArray();

//        var unsupportedConfigurations = triggerConfigurations.Where(x => x.type == null).ToArray();
//        foreach (var (id, name, _) in unsupportedConfigurations)
//        {
//            unsupportedRunIds.Add(id);
//            logger.LogError("Unknown {MessageType} ({RunId}) is ignored.", name, id);
//        }

//        return triggerConfigurations.Where(x => x.type != null).Select(x => (x.type!, x.id)).ToArray();
//    }
//}
