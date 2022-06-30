using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Unions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal class AutomationHandlers :
    IMessageHandler<AutomationQuery, AutomationModel>,
    IMessageHandler<AutomationReferencesQuery, IEnumerable<AutomationReferenceModel>>,
    IMessageHandler<AutomationCreateCommand, Guid>,
    IMessageHandler<AutomationUpdateCommand>,
    IMessageHandler<AutomationDeleteCommand>
{
    private readonly IAdminStorage<Guid, AutomationModel> storage;
    private readonly IMessagingClient client;

    public AutomationHandlers(IAdminStorage<Guid, AutomationModel> storage, IMessagingClient client)
    {
        this.storage = storage;
        this.client = client;
    }

    public async Task<AutomationModel> Handle(AutomationQuery query, CancellationToken token) =>
        await storage.GetOrDefault(query.Id, token) ?? throw new NotFoundException();

    public async Task<IEnumerable<AutomationReferenceModel>> Handle(AutomationReferencesQuery query, CancellationToken token) =>
        await storage.GetKeys(token).Select(x => new AutomationReferenceModel(x)).AsEnumerableAsync();

    public async Task<Guid> Handle(AutomationCreateCommand command, CancellationToken token)
    {
        var model = new AutomationModel(Guid.NewGuid(), command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray());
        await storage.AddOrGet(model.Id, model, token);
        return model.Id;
    }

    public async Task Handle(AutomationUpdateCommand command, CancellationToken token)
    {
        try
        {
            var jobs = command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray();
            var model = new AutomationModel(command.Id, command.Name, jobs);
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, _) => model,
                token);
        }
        catch (StorageException ex) when (ex.InnerException is NotFoundException nfe)
        {
            throw nfe;
        }
    }

    public async Task Handle(AutomationDeleteCommand command, CancellationToken token)
    {
        if(await storage.TryGet(command.Id, token) is not Some<AutomationModel>(var model))
            return;

        await Task.WhenAll(model.Jobs.Select(x => client.Request(new JobDeleteCommand(x.Id), token)));

        await storage.TryRemove(command.Id, token);
    }
}
