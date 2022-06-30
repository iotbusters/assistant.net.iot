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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal class TriggerHandlers :
    IMessageHandler<TriggerReferencesQuery, IEnumerable<TriggerReferenceModel>>,
    IMessageHandler<TriggerQuery, TriggerModel>,
    IMessageHandler<TriggerCreateCommand, Guid>,
    IMessageHandler<TriggerUpdateCommand>,
    IMessageHandler<TriggerDeleteCommand>
{
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, TriggerModel> storage;
    private readonly string timerEventName;

    public TriggerHandlers(
        IMessagingClient client,
        ITypeEncoder typeEncoder,
        IAdminStorage<Guid, TriggerModel> storage)
    {
        this.client = client;
        this.storage = storage;
        this.timerEventName = typeEncoder.Encode(typeof(TimerTriggeredEvent))
                              ?? throw new InvalidOperationException($"Failed to encode type {nameof(TimerTriggeredEvent)}.");
    }

    public async Task<IEnumerable<TriggerReferenceModel>> Handle(TriggerReferencesQuery query, CancellationToken token) =>
        await storage.GetKeys(token).Select(x => new TriggerReferenceModel(x)).AsEnumerableAsync();

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

        return model.RunId;
    }
    
    public async Task Handle(TriggerUpdateCommand command, CancellationToken token)
    {
        try
        {
            await storage.AddOrUpdate(
                command.RunId,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, old) => old.Activate(command.IsActive),
                token);
        }
        catch (StorageException ex) when (ex.InnerException is NotFoundException)
        {
            throw;
        }
    }

    public async Task Handle(TriggerDeleteCommand command, CancellationToken token) =>
        await storage.TryRemove(command.RunId, token);
}
