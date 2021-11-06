using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class AutomationHandlers :
        IMessageHandler<AutomationCreateCommand, Guid>,
        IMessageHandler<AutomationUpdateCommand>,
        IMessageHandler<AutomationDeleteCommand>,
        IMessageHandler<AutomationQuery, AutomationModel>,
        IMessageHandler<AutomationReferencesQuery, IEnumerable<AutomationReferenceModel>>
    {
        private readonly IAdminStorage<Guid, AutomationModel> storage;

        public AutomationHandlers(IAdminStorage<Guid, AutomationModel> storage) =>
            this.storage = storage;

        public async Task<Guid> Handle(AutomationCreateCommand command, CancellationToken token)
        {
            var model = new AutomationModel(Guid.NewGuid(), command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray());
            await storage.AddOrGet(model.Id, model, token);
            return model.Id;
        }

        public async Task Handle(AutomationUpdateCommand command, CancellationToken token)
        {
            var model = new AutomationModel(command.Id, command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray());
            await storage.AddOrUpdate(
                command.Id,
                addFactory: _ => throw new NotFoundException(),
                updateFactory: (_, _) => model,
                token);
        }

        public async Task Handle(AutomationDeleteCommand command, CancellationToken token) =>
            await storage.TryRemove(command.Id, token);

        public async Task<AutomationModel> Handle(AutomationQuery query, CancellationToken token) =>
            await storage.GetOrDefault(query.Id, token) ?? throw new NotFoundException();

        public async Task<IEnumerable<AutomationReferenceModel>> Handle(AutomationReferencesQuery query, CancellationToken token) =>
            await storage.GetKeys(token).Select(x => new AutomationReferenceModel(x)).AsEnumerableAsync();
    }
}
