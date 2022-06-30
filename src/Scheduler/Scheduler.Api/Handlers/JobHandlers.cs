using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal class JobHandlers :
    IMessageHandler<JobQuery, JobModel>,
    IMessageHandler<JobCreateCommand, Guid>,
    IMessageHandler<JobUpdateCommand>,
    IMessageHandler<JobDeleteCommand>
{
    private readonly IAdminStorage<Guid, JobModel> storage;

    public JobHandlers(IAdminStorage<Guid, JobModel> storage) =>
        this.storage = storage;

    public async Task<JobModel> Handle(JobQuery query, CancellationToken token) =>
        await storage.GetOrDefault(query.Id, token) ?? throw new NotFoundException();

    public async Task<Guid> Handle(JobCreateCommand command, CancellationToken token)
    {
        var model = new JobModel(id: Guid.NewGuid(), command.Name, command.Configuration);
        await storage.AddOrGet(model.Id, model, token);

        return model.Id;
    }

    public async Task Handle(JobUpdateCommand command, CancellationToken token)
    {
        try
        {
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, _) => new JobModel(command.Id, command.Name, command.Configuration),
                token);
        }
        catch (StorageException ex) when (ex.InnerException is NotFoundException nfe)
        {
            throw nfe;
        }
    }

    public async Task Handle(JobDeleteCommand command, CancellationToken token) =>
        await storage.TryRemove(command.Id, token);
}
