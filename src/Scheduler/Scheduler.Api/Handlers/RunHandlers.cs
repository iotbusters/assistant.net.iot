using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
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

internal sealed class RunHandlers :
    IMessageHandler<RunQuery, RunModel>,
    IMessageHandler<RunCreateCommand, Guid>,
    IMessageHandler<RunStartCommand>,
    IMessageHandler<RunSucceedCommand>,
    IMessageHandler<RunFailCommand>,
    IMessageHandler<RunDeleteCommand>
{
    private readonly ILogger<RunHandlers> logger;
    private readonly IMessagingClient client;
    private readonly IAdminStorage<Guid, RunModel> storage;

    public RunHandlers(
        ILogger<RunHandlers> logger,
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
        logger.LogDebug("Run(new) creating: begins.");

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

        await client.Request(new RunStartCommand(runId), token);

        logger.LogInformation("Run({RunId}) creating: ends.", runId);
        return runId;
    }

    public async Task Handle(RunStartCommand command, CancellationToken token)
    {
        logger.LogDebug("Run({RunId}) starting: begins.", command.Id);

        var run = await client.Request(new RunQuery(command.Id), token);
        switch (run.JobSnapshot.Configuration)
        {
            case JobEventConfigurationDto:
            case JobTimerConfigurationDto:
                try
                {
                    await storage.AddOrUpdate(
                        run.Id,
                        addFactory: _ => throw new NotFoundException(),
                        updateFactory: (_, old) => old.WithStatus(RunStatus.Started),
                        token);
                }
                catch (StorageException ex) when (ex.InnerException is MessageException me)
                {
                    logger.LogCritical(me, "Run({RunId}) starting: failed.", run.Id);
                    me.Throw();
                }

                await client.Request(new TriggerCreateCommand(run.Id), token);
                break;

            case JobActionConfigurationDto dto:
                try
                {
                    await client.Request(dto.Action, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Run({AutomationId}, {RunId}) starting: action failed.", run.AutomationId, run.Id);

                    await client.Request(new RunFailCommand(run.Id), token);
                    break;
                }

                await client.Request(new RunSucceedCommand(run.Id), token);
                break;

            default:
                var jobConfigurationType = run.JobSnapshot.Configuration.GetType();
                throw new MessageFailedException(
                    $"Run({run.AutomationId}, {run.Id}) starting: job type {jobConfigurationType} isn't supported.");
        }

        logger.LogInformation("Run({RunId}) starting: ends.", run.Id);
    }

    public async Task Handle(RunSucceedCommand command, CancellationToken token)
    {
        logger.LogDebug("Run({RunId}) succeeding: begins.", command.Id);

        try
        {
            var run = await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, old) => old.WithStatus(RunStatus.Succeeded), token);
            switch (run.JobSnapshot.Configuration)
            {
                case JobEventConfigurationDto:
                case JobTimerConfigurationDto:
                    await client.Request(new TriggerDeactivateCommand(run.Id), token);
                    break;

                case JobActionConfigurationDto:
                    break;

                default:
                    throw new MessageFailedException($"Run({run.AutomationId}, {run.Id}) succeeding: job type isn't supported.");
            }

            await client.Publish(new RunSucceededEvent(run.Id), token);
        }
        catch (StorageException ex) when (ex.InnerException is MessageException me)
        {
            logger.LogCritical(me, "Run({RunId}) succeeding: failed.", command.Id);
            me.Throw();
        }

        logger.LogInformation("Run({RunId}) succeeding: ends.", command.Id);
    }

    public async Task Handle(RunFailCommand command, CancellationToken token)
    {
        logger.LogDebug("Run({RunId}) failing: begins.", command.Id);

        try
        {
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: async (_, old) =>
                {
                    var run = old.WithStatus(RunStatus.Failed);
                    switch (run.JobSnapshot.Configuration)
                    {
                        case JobEventConfigurationDto:
                        case JobTimerConfigurationDto:
                            await client.Request(new TriggerDeactivateCommand(run.Id), token);
                            await client.Publish(new RunFailedEvent(run.Id), token);
                            return run;

                        case JobActionConfigurationDto:
                            await client.Publish(new RunFailedEvent(run.Id), token);
                            return run;

                        default:
                            throw new MessageFailedException($"Run({run.AutomationId}, {run.Id}) failing: job type isn't supported.");
                    }
                }, token);
        }
        catch (StorageException ex) when(ex.InnerException is MessageException me)
        {
            logger.LogCritical(me, "Run({RunId}) updating: not found.", command.Id);
            me.Throw();
        }

        logger.LogInformation("Run({RunId}) failing: ends.", command.Id);
    }

    public async Task Handle(RunDeleteCommand command, CancellationToken token)
    {
        logger.LogDebug("Run({RunId}) deleting: begins.", command.Id);

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

        logger.LogInformation("Run({RunId}) failing: ends.", command.Id);
    }
}
