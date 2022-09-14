using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal sealed class JobHandlers :
    IMessageHandler<JobQuery, JobModel>,
    IMessageHandler<JobCreateCommand, Guid>,
    IMessageHandler<JobUpdateCommand>,
    IMessageHandler<JobDeleteCommand>
{
    private readonly ILogger<JobHandlers> logger;
    private readonly IAdminStorage<Guid, JobModel> storage;

    public JobHandlers(
        ILogger<JobHandlers> logger,
        IAdminStorage<Guid, JobModel> storage)
    {
        this.logger = logger;
        this.storage = storage;
    }

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
            logger.LogCritical("Job({JobId}, {JobName}) updating: not found.", command.Id, command.Name);
            nfe.Throw();
        }
    }

    public async Task Handle(JobDeleteCommand command, CancellationToken token) =>
        await storage.TryRemove(command.Id, token);
}
