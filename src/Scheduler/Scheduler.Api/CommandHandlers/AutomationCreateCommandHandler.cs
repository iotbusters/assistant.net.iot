using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.CommandHandlers
{
    internal class AutomationCreateCommandHandler : IMessageHandler<AutomationCreateCommand, Guid>
    {
        private readonly IStorage<Guid, AutomationModel> storage;

        public AutomationCreateCommandHandler(IStorage<Guid, AutomationModel> storage) =>
            this.storage = storage;

        public Task<Guid> Handle(AutomationCreateCommand command)
        {
            var model = new AutomationModel(Guid.NewGuid(), command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)));
            return storage.AddOrGet(model.Id, model).MapSuccess(x => x.Id);
        }
    }
}