using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal class RunHandlers :
    IMessageHandler<RunCreateCommand, Guid>,
    IMessageHandler<RunUpdateCommand>,
    IMessageHandler<RunDeleteCommand>,
    IMessageHandler<RunQuery, RunModel>
{
    private readonly ILogger<RunHandlers> logger;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, RunModel> storage;

    public RunHandlers(
        ILogger<RunHandlers> logger,
        IMessagingClient client,
        IAdminStorage<Guid, RunModel> storage)
    {
        this.logger = logger;
        this.client = client;
        this.storage = storage;
    }

    public async Task<Guid> Handle(RunCreateCommand command, CancellationToken token)
    {
        var automation = await client.Request(new AutomationQuery(command.AutomationId), token);
        if (!automation.Jobs.Any())
            throw new InvalidRequestException("Automation doesn't have any jobs.");

        var jobTasks = automation.Jobs.Select(x => client.Request(new JobQuery(x.Id), token));
        var jobs = await Task.WhenAll(jobTasks);

        var runTasks = jobs.Reverse().Aggregate(new List<RunModel>(jobs.Length), (list, job) =>
        {
            list.Add(new RunModel(id: Guid.NewGuid(), nextRunId: list.LastOrDefault()?.Id, automation.Id, job));
            return list;
        }).Select(x => storage.AddOrGet(x.Id, x, token));

        var runs = await Task.WhenAll(runTasks);
        var runId = runs.Last().Id;

        var started = new RunStatusDto(RunStatus.Started);
        await client.Request(new RunUpdateCommand(runId, started), token);

        return runId;
    }

    public async Task Handle(RunUpdateCommand command, CancellationToken token)
    {
        var run = await storage.AddOrUpdate(
            command.Id,
            addFactory: _ => throw new NotFoundException(),
            updateFactory: (_, old) => old.WithStatus(command.Status),
            token);

        switch (run)
        {
            case {JobSnapshot: JobTriggerEventModel, Status.Value: RunStatus.Started}:
                await client.Request(new TriggerCreateCommand(run.Id), token);
                break;

            case {JobSnapshot: JobTriggerEventModel, Status.Value: RunStatus.Succeeded}:
                await client.Request(new TriggerUpdateCommand(run.Id, isActive: false), token);
                await client.Publish(new RunSucceededEvent(run.Id), token);
                break;

            case {JobSnapshot: JobTriggerEventModel, Status.Value: RunStatus.Failed}:
                await client.Request(new TriggerUpdateCommand(run.Id, isActive: false), token);
                await client.Publish(new RunFailedEvent(run.Id), token);
                break;

            case {JobSnapshot: JobActionModel job, Status.Value: RunStatus.Started}:
                try
                {
                    await client.Request(job.Action, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Run({AutomationId}/{RunId}): failed.", run.AutomationId, run.Id);
                    await client.Publish(new RunFailedEvent(run.Id), token);
                    break;
                }

                await client.Publish(new RunSucceededEvent(run.Id), token);
                break;

            case {JobSnapshot: JobActionModel, Status.Value: RunStatus.Succeeded}:
                await client.Publish(new RunSucceededEvent(run.Id), token);
                break;

            case {JobSnapshot: JobActionModel, Status.Value: RunStatus.Failed}:
                await client.Publish(new RunFailedEvent(run.Id), token);
                break;
        }
    }

    public async Task Handle(RunDeleteCommand command, CancellationToken token) =>
        await storage.TryRemove(command.Id, token);

    public async Task<RunModel> Handle(RunQuery query, CancellationToken token) =>
        await storage.GetOrDefault(query.Id, token) ?? throw new NotFoundException();
}
