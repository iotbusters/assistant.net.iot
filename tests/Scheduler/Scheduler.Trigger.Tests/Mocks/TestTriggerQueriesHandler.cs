﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks
{
    public class TestTriggerQueriesHandler :
        IMessageHandler<TriggerReferencesQuery, TriggerReferenceModel[]>,
        IMessageHandler<TriggerQuery, TriggerModel>
    {
        private readonly Dictionary<Guid, TriggerModel> triggers = new();

        public Task<TriggerReferenceModel[]> Handle(TriggerReferencesQuery message, CancellationToken token) =>
            Task.FromResult(triggers.Values.Select(x => new TriggerReferenceModel(x.RunId)).ToArray());

        public Task<TriggerModel> Handle(TriggerQuery message, CancellationToken token)
        {
            if (triggers.TryGetValue(message.RunId, out var response))
                return Task.FromResult(response);
            throw new NotFoundException();
        }

        public void Add(Type messageType)
        {
            var id = Guid.NewGuid();
            triggers.Add(id, new TriggerModel(
                runId: id,
                isActive: true,
                triggerEventName: messageType.Name,
                triggerEventMask: new Dictionary<string, string>()));
        }

        public void Remove(Type messageType)
        {
            var id = triggers.Where(x => x.Value.TriggerEventName == messageType.Name).Select(x => x.Key).FirstOrDefault();
            if (id != Guid.Empty)
                triggers.Remove(id);
        }
    }
}
