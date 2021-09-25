using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
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
    internal class RunCreateCommandHandler : IMessageHandler<RunCreateCommand, Guid>
    {
        private readonly IMessagingClient client;
        private readonly IStorage<Guid, RunModel> storage;

        public RunCreateCommandHandler(
            IMessagingClient client,
            IStorage<Guid, RunModel> storage)
        {
            this.client = client;
            this.storage = storage;
        }

        public async Task<Guid> Handle(RunCreateCommand command, CancellationToken token)
        {
            var automation = await client.SendAs(new AutomationQuery(command.AutomationId));

            var jobTasks = automation.Jobs.Select(x => client.SendAs(new JobQuery(x.Id)));
            var jobs = await Task.WhenAll(jobTasks);

            var runTasks = jobs.Reverse().Aggregate(new List<RunModel>(jobs.Length), (list, job) =>
            {
                list.Add(new RunModel(Guid.NewGuid(), list.LastOrDefault()?.Id, automation.Id, job));
                return list;
            }).Select(x => storage.AddOrGet(x.Id, x, token));
            var runs = await Task.WhenAll(runTasks);

            return runs.Last().Id;
        }
    }
}
