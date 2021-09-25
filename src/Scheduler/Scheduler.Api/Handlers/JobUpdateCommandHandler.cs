using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class JobUpdateCommandHandler : IMessageHandler<JobUpdateCommand>
    {
        private readonly IStorage<Guid, JobModel> storage;

        public JobUpdateCommandHandler(IStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public async Task Handle(JobUpdateCommand command, CancellationToken token) =>
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, _) => new JobModel(
                    command.Id,
                    command.Name,
                    command.Trigger,
                    command.TriggerEventMask,
                    command.Type,
                    command.Parameters),
                token);
    }
}
