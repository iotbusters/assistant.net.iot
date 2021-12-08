using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Internal
{
    internal class TriggerPollingService : BackgroundService
    {
        private readonly TimeSpan pollingDelayTime = TimeSpan.FromMinutes(5);
        private readonly HashSet<Guid> unsupportedRunId = new();
        private Dictionary<Guid, Type> events = new();

        private readonly ILogger logger;
        private readonly ReloadableOptionsSource eventSource;
        private readonly IMessagingClient client;
        private readonly ISystemLifetime lifetime;
        private readonly ITypeEncoder typeEncoder;

        public TriggerPollingService(
            ILogger<TriggerPollingService> logger,
            ReloadableOptionsSource eventSource,
            ITypeEncoder typeEncoder,
            IMessagingClient client,
            ISystemLifetime lifetime)
        {
            this.logger = logger;
            this.eventSource = eventSource;
            this.typeEncoder = typeEncoder;
            this.client = client;
            this.lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            logger.LogInformation("Start polling triggers resource.");

            while (!lifetime.Stopping.IsCancellationRequested)
            {
                await ReloadAsync(lifetime.Stopping);
                await Task.Delay(pollingDelayTime, lifetime.Stopping);
            }

            logger.LogInformation("Stop polling triggers resource.");
        }

        public async Task ReloadAsync(CancellationToken systemToken)
        {
            logger.LogDebug("Reloading supported trigger events to handle.");

            var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
            var token = CancellationTokenSource.CreateLinkedTokenSource(systemToken, timeoutToken).Token;

            var references = await client.Request(new TriggerReferencesQuery(), token);
            var runIds = references.Select(x => x.RunId).Except(unsupportedRunId).ToHashSet();

            var currentEvents = events;
            if (runIds.SequenceEqual(currentEvents.Keys))
                return;

            var triggerQueryTasks = runIds.Except(currentEvents.Keys).Select(x => client.Request(new TriggerQuery(x), token));
            var triggers = await Task.WhenAll(triggerQueryTasks);

            var newEvents = triggers
                .Select(x => (id: x.RunId, name: x.TriggerEventName, type: typeEncoder.Decode(x.TriggerEventName)))
                .ToArray();

            var unsupportedEvents = newEvents.Where(x => x.type == null).ToArray();
            foreach (var @event in unsupportedEvents)
            {
                unsupportedRunId.Add(@event.id);
                logger.LogError("Unknown {EventName} ({RunId}) is ignored.", @event.name, @event.id);
            }

            var knownEvents = newEvents.Where(x => x.type != null).ToDictionary(x => x.id, x => x.type!);
            if (!knownEvents.Any())
                logger.LogWarning("No new events were found.");

            events = (
                    from id in runIds
                    join kid in knownEvents on id equals kid.Key into kids
                    join cid in currentEvents on id equals cid.Key into cids
                    let type = kids.FirstOrDefault().Value ?? cids.FirstOrDefault().Value
                    where type != null
                    select (id, type))
                .ToDictionary(x => x.id, x => x.type!);
            eventSource.Reload(events.Values);

            logger.LogDebug("Reloaded supported trigger events to handle.");
        }
    }
}
