using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Exceptions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.CommandHandlers
{
    internal class AutomationUpdateCommandHandler : IMessageHandler<AutomationUpdateCommand>
    {
        private readonly IStorage<Guid, AutomationModel> storage;

        public AutomationUpdateCommandHandler(IStorage<Guid, AutomationModel> storage) =>
            this.storage = storage;

        public Task Handle(AutomationUpdateCommand command)
        {
            var model = new AutomationModel(command.Id, command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)));
            return storage.AddOrUpdate(
                    command.Id,
                    addFactory: _ => throw new NotFoundException(),
                    updateFactory: (_, _) => model);
        }
    }
}