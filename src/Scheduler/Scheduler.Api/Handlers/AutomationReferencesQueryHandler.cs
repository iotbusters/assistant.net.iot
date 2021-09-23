using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class AutomationReferencesQueryHandler : IMessageHandler<AutomationReferencesQuery, IEnumerable<AutomationReferenceModel>>
    {
        private readonly IAdminStorage<Guid, AutomationModel> storage;

        public AutomationReferencesQueryHandler(IAdminStorage<Guid, AutomationModel> storage) =>
            this.storage = storage;

        public Task<IEnumerable<AutomationReferenceModel>> Handle(AutomationReferencesQuery command, CancellationToken token) =>
            storage.GetKeys(token).Select(x => new AutomationReferenceModel(x)).AsEnumerableAsync();
    }
}
