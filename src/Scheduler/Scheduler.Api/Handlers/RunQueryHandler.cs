using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class RunQueryHandler : IMessageHandler<RunQuery, RunModel>
    {
        private readonly IStorage<Guid, RunModel> storage;

        public RunQueryHandler(IStorage<Guid, RunModel> storage) =>
            this.storage = storage;

        public async Task<RunModel> Handle(RunQuery command, CancellationToken token) =>
            await storage.GetOrDefault(command.Id, token) ?? throw new NotFoundException();
    }
}
