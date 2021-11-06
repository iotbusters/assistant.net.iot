using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class JobHandlers :
        IMessageHandler<JobActionCreateCommand, Guid>,
        IMessageHandler<JobTriggerCreateCommand, Guid>,
        IMessageHandler<JobActionUpdateCommand>,
        IMessageHandler<JobTriggerUpdateCommand>,
        IMessageHandler<JobDeleteCommand>,
        IMessageHandler<JobQuery, JobModel>
    {
        private readonly IAdminStorage<Guid, JobModel> storage;

        public JobHandlers(IAdminStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public async Task<Guid> Handle(JobActionCreateCommand command, CancellationToken token)
        {
            var model = new JobActionModel(
                id: Guid.NewGuid(),
                command.Name,
                command.Action);
            await storage.AddOrGet(model.Id, model, token);

            return model.Id;
        }

        public async Task<Guid> Handle(JobTriggerCreateCommand command, CancellationToken token)
        {
            var model = new JobTriggerModel(
                id: Guid.NewGuid(),
                command.Name,
                command.TriggerEventName,
                command.TriggerEventMask);
            await storage.AddOrGet(model.Id, model, token);

            return model.Id;
        }

        public async Task Handle(JobActionUpdateCommand command, CancellationToken token) =>
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, _) => new JobActionModel(command.Id, command.Name, command.Action),
                token);

        public async Task Handle(JobTriggerUpdateCommand command, CancellationToken token) =>
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, _) => new JobTriggerModel(command.Id, command.Name, command.TriggerEventName, command.TriggerEventMask),
                token);

        public async Task Handle(JobDeleteCommand command, CancellationToken token) =>
            await storage.TryRemove(command.Id, token);

        public async Task<JobModel> Handle(JobQuery query, CancellationToken token) =>
            await storage.GetOrDefault(query.Id, token) ?? throw new NotFoundException();
    }
}
