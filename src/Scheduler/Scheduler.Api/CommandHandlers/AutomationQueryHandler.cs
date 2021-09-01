using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Exceptions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.CommandHandlers
{
    internal class AutomationQueryHandler : IMessageHandler<AutomationQuery, AutomationModel>
    {
        private readonly IStorage<Guid, AutomationModel> storage;

        public AutomationQueryHandler(IStorage<Guid, AutomationModel> storage) =>
            this.storage = storage;

        public Task<AutomationModel> Handle(AutomationQuery command) =>
            storage.GetOrDefault(command.Id).MapSuccess(x => x ?? throw new NotFoundException());
    }
}