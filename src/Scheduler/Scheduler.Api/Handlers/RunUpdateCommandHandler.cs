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
    internal class RunUpdateCommandHandler : IMessageHandler<RunUpdateCommand>
    {
        private readonly IStorage<Guid, RunModel> storage;

        public RunUpdateCommandHandler(IStorage<Guid, RunModel> storage) =>
            this.storage = storage;

        public async Task Handle(RunUpdateCommand command, CancellationToken token) =>
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, old) => old.WithStatus(command.Status),
                token);
    }
}
