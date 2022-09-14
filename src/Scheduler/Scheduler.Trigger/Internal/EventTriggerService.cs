using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Internal;

internal class EventTriggerService : IEventTriggerService
{
    private readonly ILogger<EventTriggerService> logger;
    private readonly ReloadableEventTriggerOptionsSource eventTriggerOptionsSource;
    private readonly ITypeEncoder typeEncoder;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, TriggerTimerModel> storage;

    public EventTriggerService(
        ILogger<EventTriggerService> logger,
        ReloadableEventTriggerOptionsSource eventTriggerOptionsSource,
        ITypeEncoder typeEncoder,
        IMessagingClient client,
        IAdminStorage<Guid, TriggerTimerModel> storage)
    {
        this.logger = logger;
        this.eventTriggerOptionsSource = eventTriggerOptionsSource;
        this.typeEncoder = typeEncoder;
        this.client = client;
        this.storage = storage;
    }

    public async Task ReconfigureEventTriggers(CancellationToken token)
    {
        logger.LogDebug("Reload triggers: begins.");

        var runIds = await storage.GetKeys(token).ToArrayAsync(token);

        var triggerTypeQueryTasks = runIds.Select(x => GetEventTriggerType(x, token));
        var triggerTypes = await Task.WhenAll(triggerTypeQueryTasks);
        var triggers = runIds.Zip(triggerTypes).Where(x => x.Second != null).ToArray();

        if (!triggers.Any())
        {
            logger.LogWarning("Reload triggers: not found.");
            return;
        }

        var eventTriggers = triggers
            .GroupBy(x => x.Second!, x => x.First)
            .ToDictionary(x => x.Key, x => (IList<Guid>)x.ToList());
        eventTriggerOptionsSource.Reload(eventTriggers);

        logger.LogInformation("Reload triggers: ends.");
    }

    public async Task ConfigureEventTrigger(Guid runId, CancellationToken token)
    {
        using var runScope = logger.BeginPropertyScope("RunId", runId);

        logger.LogDebug("Reload trigger: begins.");

        var eventTriggerType = await GetEventTriggerType(runId, token);
        if (eventTriggerType == null)
        {
            logger.LogWarning("Reload trigger: not found.");
            return;
        }

        eventTriggerOptionsSource.Add(eventTriggerType, runId);

        logger.LogInformation("Reload trigger: ends.");
    }

    private async Task<Type?> GetEventTriggerType(Guid runId, CancellationToken token)
    {
        var trigger = await client.Request(new TriggerQuery(runId), token);
        return typeEncoder.Decode(trigger.TriggerEventName);
    }
}
