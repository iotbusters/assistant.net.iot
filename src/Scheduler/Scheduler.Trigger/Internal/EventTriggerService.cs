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
    private readonly ReloadableEventTriggerOptionsSource optionsSource;
    private readonly ITypeEncoder typeEncoder;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, TriggerTimerModel> storage;

    public EventTriggerService(
        ILogger<EventTriggerService> logger,
        ReloadableEventTriggerOptionsSource optionsSource,
        ITypeEncoder typeEncoder,
        IMessagingClient client,
        IAdminStorage<Guid, TriggerTimerModel> storage)
    {
        this.logger = logger;
        this.optionsSource = optionsSource;
        this.typeEncoder = typeEncoder;
        this.client = client;
        this.storage = storage;
    }

    public async Task ReloadEventTriggers(CancellationToken token)
    {
        logger.LogDebug("Configure triggers: begins.");

        var runIds = await storage.GetKeys(token).ToArrayAsync(token);

        var triggerTypeQueryTasks = runIds.Select(x => GetEventTriggerType(x, token));
        var triggerTypes = await Task.WhenAll(triggerTypeQueryTasks);
        var triggers = runIds.Zip(triggerTypes).Where(x => x.Second != null).ToArray();

        if (!triggers.Any())
        {
            logger.LogWarning("Configure triggers: not found.");
            return;
        }

        var eventTriggers = triggers
            .GroupBy(x => x.Second!, x => x.First)
            .ToDictionary(x => x.Key, x => (ISet<Guid>)x.ToHashSet());
        optionsSource.Reload(eventTriggers);

        logger.LogInformation("Configure triggers: ends.");
    }

    public async Task AddEventTrigger(Guid runId, CancellationToken token)
    {
        using var runScope = logger.BeginPropertyScope("RunId", runId);

        logger.LogDebug("Add trigger: begins.");

        var eventTriggerType = await GetEventTriggerType(runId, token);
        if (eventTriggerType == null)
        {
            logger.LogWarning("Add trigger: not found.");
            return;
        }

        optionsSource.Add(eventTriggerType, runId);

        logger.LogInformation("Add trigger: ends.");
    }

    public async Task RemoveEventTrigger(Guid runId, CancellationToken token)
    {
        using var runScope = logger.BeginPropertyScope("RunId", runId);

        logger.LogDebug("Remove trigger: begins.");

        var eventTriggerType = await GetEventTriggerType(runId, token);
        if (eventTriggerType == null)
        {
            logger.LogWarning("Remove trigger: not found.");
            return;
        }

        optionsSource.Remove(eventTriggerType, runId);

        logger.LogInformation("Remove trigger: ends.");
    }

    private async Task<Type?> GetEventTriggerType(Guid runId, CancellationToken token)
    {
        var trigger = await client.Request(new TriggerQuery(runId), token);
        return typeEncoder.Decode(trigger.TriggerEventName);
    }
}
