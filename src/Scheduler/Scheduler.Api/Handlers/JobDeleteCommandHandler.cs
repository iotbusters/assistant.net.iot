using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class JobDeleteCommandHandler : IMessageHandler<JobDeleteCommand>
    {
        private readonly IStorage<Guid, JobModel> storage;

        public JobDeleteCommandHandler(IStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public async Task Handle(JobDeleteCommand command, CancellationToken token) =>
            await storage.TryRemove(command.Id, token);
    }
}
