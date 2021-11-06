using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class RunHandlers :
        IMessageHandler<RunCreateCommand, Guid>,
        IMessageHandler<RunUpdateCommand>,
        IMessageHandler<RunDeleteCommand>,
        IMessageHandler<RunQuery, RunModel>
    {
        private readonly IMessagingClient client;
        private readonly IStorage<Guid, RunModel> storage;

        public RunHandlers(
            IMessagingClient client,
            IStorage<Guid, RunModel> storage)
        {
            this.client = client;
            this.storage = storage;
        }

        public async Task<Guid> Handle(RunCreateCommand command, CancellationToken token)
        {
            var automation = await client.Request(new AutomationQuery(command.AutomationId), token);

            var jobTasks = automation.Jobs.Select(x => client.Request(new JobQuery(x.Id), token));
            var jobs = await Task.WhenAll(jobTasks);

            var runTasks = jobs.Reverse().Aggregate(new List<RunModel>(jobs.Length), (list, job) =>
            {
                list.Add(new RunModel(Guid.NewGuid(), list.LastOrDefault()?.Id, automation.Id, job));
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
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, old) => old.WithStatus(command.Status),
                token);

            switch (command.Status.Value)
            {
                case RunStatus.Started:
                    await client.Request(new TriggerCreateCommand(command.Id), token);
                    break;
                case RunStatus.Succeeded:
                    await client.Publish(new RunSucceededEvent(command.Id), token);
                    break;
                case RunStatus.Failed:
                    await client.Publish(new RunFailedEvent(command.Id), token);
                    break;
            }
        }

        public async Task Handle(RunDeleteCommand command, CancellationToken token) =>
            await storage.TryRemove(command.Id, token);

        public async Task<RunModel> Handle(RunQuery query, CancellationToken token) =>
            await storage.GetOrDefault(query.Id, token) ?? throw new NotFoundException();
    }
}
