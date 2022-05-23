using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal class TriggerHandlers :
    IMessageHandler<TriggerCreateCommand, Guid>,
    IMessageHandler<TriggerUpdateCommand>,
    IMessageHandler<TriggerDeleteCommand>,
    IMessageHandler<TriggerQuery, TriggerModel>,
    IMessageHandler<TriggerReferencesQuery, IEnumerable<TriggerReferenceModel>>
{
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, TriggerModel> storage;

    public TriggerHandlers(
        IMessagingClient client,
        IAdminStorage<Guid, TriggerModel> storage)
    {
        this.client = client;
        this.storage = storage;
    }

    public async Task<Guid> Handle(TriggerCreateCommand command, CancellationToken token)
    {
        var run = await client.Request(new RunQuery(command.RunId), token);

        if (run.JobSnapshot is not JobTriggerEventModel job)
            throw new MessageContractException($"Run({run.Id}) doesn't have a trigger.");

        var model = new TriggerModel(run.Id, isActive: true, job.TriggerEventName, job.TriggerEventMask);
        await storage.AddOrGet(model.RunId, model, token);

        return model.RunId;
    }

    public async Task Handle(TriggerUpdateCommand command, CancellationToken token) =>
        await storage.AddOrUpdate(
            command.RunId,
            addFactory: _ => throw new NotFoundException(),
            updateFactory: (_, old) => old.Activate(command.IsActive),
            token);

    public async Task Handle(TriggerDeleteCommand command, CancellationToken token) =>
        await storage.TryRemove(command.RunId, token);

    public async Task<TriggerModel> Handle(TriggerQuery query, CancellationToken token) =>
        await storage.GetOrDefault(query.RunId, token) ?? throw new NotFoundException();

    public async Task<IEnumerable<TriggerReferenceModel>> Handle(TriggerReferencesQuery query, CancellationToken token) =>
        await storage.GetKeys(token).Select(x => new TriggerReferenceModel(x)).AsEnumerableAsync();
}
