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
    internal class AutomationQueryHandler : IMessageHandler<AutomationQuery, AutomationModel>
    {
        private readonly IStorage<Guid, AutomationModel> storage;

        public AutomationQueryHandler(IStorage<Guid, AutomationModel> storage) =>
            this.storage = storage;

        public async Task<AutomationModel> Handle(AutomationQuery command, CancellationToken token) =>
            await storage.GetOrDefault(command.Id, token) ?? throw new NotFoundException();
    }
}
