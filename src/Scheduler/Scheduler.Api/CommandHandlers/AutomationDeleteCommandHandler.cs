using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.CommandHandlers
{
    internal class AutomationDeleteCommandHandler : IMessageHandler<AutomationDeleteCommand>
    {
        private readonly IStorage<Guid, JobModel> storage;

        public AutomationDeleteCommandHandler(IStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public async Task Handle(AutomationDeleteCommand command) =>
            await storage.TryRemove(command.Id);
    }
}