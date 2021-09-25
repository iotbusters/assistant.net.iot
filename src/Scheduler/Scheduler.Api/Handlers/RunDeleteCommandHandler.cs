using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class RunDeleteCommandHandler : IMessageHandler<RunDeleteCommand>
    {
        private readonly IStorage<Guid, RunModel> storage;

        public RunDeleteCommandHandler(IStorage<Guid, RunModel> storage) =>
            this.storage = storage;

        public async Task Handle(RunDeleteCommand command, CancellationToken token) =>
            await storage.TryRemove(command.Id, token);
    }
}
