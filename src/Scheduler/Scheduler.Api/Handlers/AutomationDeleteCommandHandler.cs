using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class AutomationDeleteCommandHandler : IMessageHandler<AutomationDeleteCommand>
    {
        private readonly IStorage<Guid, JobModel> storage;

        public AutomationDeleteCommandHandler(IStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public Task Handle(AutomationDeleteCommand command, CancellationToken token) =>
            storage.TryRemove(command.Id, token);
    }
}