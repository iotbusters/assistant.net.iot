using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Exceptions;
using Assistant.Net.Scheduler.Api.Models;
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

        public Task Handle(JobUpdateCommand command, CancellationToken token)
        {
            var model = new JobModel(command.Id, command.Name, command.Trigger, command.TriggerEventMask, command.Type, command.Parameters);
            return storage.AddOrUpdate(
                    model.Id,
                    addFactory: _ => throw new NotFoundException(),
                    updateFactory: (_, _) => model,
                    token);
        }
    }
}