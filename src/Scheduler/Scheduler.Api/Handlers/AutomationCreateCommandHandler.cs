using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class AutomationCreateCommandHandler : IMessageHandler<AutomationCreateCommand, Guid>
    {
        private readonly IStorage<Guid, AutomationModel> storage;

        public AutomationCreateCommandHandler(IStorage<Guid, AutomationModel> storage) =>
            this.storage = storage;

        public async Task<Guid> Handle(AutomationCreateCommand command, CancellationToken token)
        {
            var model = new AutomationModel(Guid.NewGuid(), command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)));
            await storage.AddOrGet(model.Id, model, token);
            return model.Id;
        }
    }
}
