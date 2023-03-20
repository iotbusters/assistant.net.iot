using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal sealed class TriggerHandlers :
    IMessageHandler<TriggerReferencesQuery, IEnumerable<TriggerReferenceModel>>,
    IMessageHandler<TriggerQuery, TriggerModel>,
    IMessageHandler<TriggerCreateCommand, Guid>,
    IMessageHandler<TriggerDeactivateCommand>,
    IMessageHandler<TriggerDeleteCommand>
{
    private readonly ILogger<TriggerHandlers> logger;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, TriggerModel> storage;
    private readonly string timerEventName;

    public TriggerHandlers(
        ILogger<TriggerHandlers> logger,
        IMessagingClient client,
        ITypeEncoder typeEncoder,
        IAdminStorage<Guid, TriggerModel> storage)
    {
        this.logger = logger;
        this.client = client;
        this.storage = storage;
        this.timerEventName = typeEncoder.Encode(typeof(TimerTriggeredEvent))
                              ?? throw new InvalidOperationException($"Failed to encode type {nameof(TimerTriggeredEvent)}.");
    }

    public async Task<IEnumerable<TriggerReferenceModel>> Handle(TriggerReferencesQuery query, CancellationToken token) =>
        await storage.GetKeys(token).Select(x => new TriggerReferenceModel(x)).AsEnumerableAsync(token);

    public async Task<TriggerModel> Handle(TriggerQuery query, CancellationToken token) =>
        await storage.GetOrDefault(query.RunId, token) ?? throw new NotFoundException();

    public async Task<Guid> Handle(TriggerCreateCommand command, CancellationToken token)
    {
        var run = await client.Request(new RunQuery(command.RunId), token);

        var model = run.JobSnapshot.Configuration switch
        {
            JobEventConfigurationDto dto => new TriggerModel(
                run.Id,
                isActive: true,
                dto.EventName,
                dto.EventMask),
            JobTimerConfigurationDto => new TriggerModel(
                run.Id,
                isActive: true,
                timerEventName,
                triggerEventMask: new Dictionary<string, string> {[nameof(TimerTriggeredEvent.RunId)] = run.Id.ToString()}),
            _ => throw new MessageContractException($"Run({run.Id}) doesn't have a trigger.")
        };
        await storage.AddOrGet(model.RunId, model, token);
        await client.Publish(new TriggerCreatedEvent(model.RunId), token);

        return model.RunId;
    }

    public async Task Handle(TriggerDeactivateCommand command, CancellationToken token)
    {
        try
        {
            await storage.AddOrUpdate(
                command.RunId,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, old) => old.Activate(isActive: false),
                token);
        }
        catch (StorageException ex) when (ex.InnerException is NotFoundException nfe)
        {
            logger.LogCritical("Trigger({RunId}) deactivation: not found.", command.RunId);
            nfe.Throw();
        }

        await client.Publish(new TriggerDeactivatedEvent(command.RunId), token);
    }

    public async Task Handle(TriggerDeleteCommand command, CancellationToken token)
    {
        await client.Publish(new TriggerDeactivatedEvent(command.RunId), token);

        await storage.TryRemove(command.RunId, token);
    }
}
