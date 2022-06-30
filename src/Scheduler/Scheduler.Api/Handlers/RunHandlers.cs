using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers;

internal class RunHandlers :
    IMessageHandler<RunQuery, RunModel>,
    IMessageHandler<RunCreateCommand, Guid>,
    IMessageHandler<RunUpdateCommand>,
    IMessageHandler<RunDeleteCommand>
{
    private readonly ILogger<RunHandlers> logger;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, RunModel> storage;

    public RunHandlers(ILogger<RunHandlers> logger,
        IAdminStorage<Guid, RunModel> storage,
        IMessagingClient client)
    {
        this.logger = logger;
        this.client = client;
        this.storage = storage;
    }

    public async Task<RunModel> Handle(RunQuery query, CancellationToken token) =>
        await storage.GetOrDefault(query.Id, token) ?? throw new NotFoundException();

    public async Task<Guid> Handle(RunCreateCommand command, CancellationToken token)
    {
        var automation = await client.Request(new AutomationQuery(command.AutomationId), token);
        if (!automation.Jobs.Any())
            throw new InvalidRequestException("Automation doesn't have any jobs.");

        var jobTasks = automation.Jobs.Select(x => client.Request(new JobQuery(x.Id), token));
        var jobs = await Task.WhenAll(jobTasks);

        var runs = jobs.Reverse().Aggregate(new List<RunModel>(jobs.Length), (list, job) =>
        {
            list.Add(new RunModel(id: Guid.NewGuid(), nextRunId: list.LastOrDefault()?.Id, automation.Id, job));
            return list;
        });
        runs.Reverse();
        var runId = runs.First().Id;

        await Task.WhenAll(runs.Select(x => storage.AddOrGet(x.Id, x, token)));

        var started = new RunStatusDto(RunStatus.Started);
        await client.Request(new RunUpdateCommand(runId, started), token);

        return runId;
    }

    public async Task Handle(RunUpdateCommand command, CancellationToken token)
    {
        try
        {
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: async (_, old) =>
                {
                    var run = old.WithStatus(command.Status);
                    await Update(run, token);
                    return run;
                }, token);
        }
        catch (StorageException ex) when(ex.InnerException is NotFoundException nfe)
        {
            throw nfe;
        }
    }

    public async Task Handle(RunDeleteCommand command, CancellationToken token)
    {
        var runId = command.Id;
        var stack = new Stack<RunModel>();
        while (await storage.TryGet(runId, token) is Some<RunModel>({NextRunId : not null} run))
        {
            stack.Push(run);
            runId = run.NextRunId.Value;
        }

        while (stack.TryPop(out var run))
        {
            if (run is {JobSnapshot.Configuration: JobEventConfigurationDto or JobTimerConfigurationDto})
                await client.Request(new TriggerDeleteCommand(run.Id), token);

            await storage.TryRemove(run.Id, token);
        }
    }

    private async Task Update(RunModel run, CancellationToken token)
    {
        switch (run)
        {
            case {JobSnapshot.Configuration: JobEventConfigurationDto, Status.Value: RunStatus.Started}:
            case {JobSnapshot.Configuration: JobTimerConfigurationDto, Status.Value: RunStatus.Started}:
                await client.Request(new TriggerCreateCommand(run.Id), token);
                break;

            case {JobSnapshot.Configuration: JobEventConfigurationDto, Status.Value: RunStatus.Succeeded}:
            case {JobSnapshot.Configuration: JobTimerConfigurationDto, Status.Value: RunStatus.Succeeded}:
                await client.Request(new TriggerUpdateCommand(run.Id, isActive: false), token);
                await client.Publish(new RunSucceededEvent(run.Id), token);
                break;

            case {JobSnapshot.Configuration: JobEventConfigurationDto, Status.Value: RunStatus.Failed}:
            case {JobSnapshot.Configuration: JobTimerConfigurationDto, Status.Value: RunStatus.Failed}:
                await client.Request(new TriggerUpdateCommand(run.Id, isActive: false), token);
                await client.Publish(new RunFailedEvent(run.Id), token);
                break;

            case {JobSnapshot.Configuration: JobActionConfigurationDto dto, Status.Value: RunStatus.Started}:
                try
                {
                    await client.Request(dto.Action, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Run({AutomationId}/{RunId}): failed.", run.AutomationId, run.Id);
                    await client.Publish(new RunFailedEvent(run.Id), token);
                    break;
                }

                await client.Publish(new RunSucceededEvent(run.Id), token);
                break;

            case {JobSnapshot.Configuration: JobActionConfigurationDto, Status.Value: RunStatus.Succeeded}:
                await client.Publish(new RunSucceededEvent(run.Id), token);
                break;

            case {JobSnapshot.Configuration: JobActionConfigurationDto, Status.Value: RunStatus.Failed}:
                await client.Publish(new RunFailedEvent(run.Id), token);
                break;
        }
    }
}
