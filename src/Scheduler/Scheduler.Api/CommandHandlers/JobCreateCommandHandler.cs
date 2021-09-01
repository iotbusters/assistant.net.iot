using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.CommandHandlers
{
    internal class JobCreateCommandHandler : IMessageHandler<JobCreateCommand, Guid>
    {
        private readonly IStorage<Guid, JobModel> storage;

        public JobCreateCommandHandler(IStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public Task<Guid> Handle(JobCreateCommand command)
        {
            var model = new JobModel(Guid.NewGuid(), command.Name, command.Trigger, command.TriggerEventMask, command.Type, command.Parameters);
            return storage.AddOrGet(model.Id, model).MapSuccess(x => x.Id);
        }
    }
}