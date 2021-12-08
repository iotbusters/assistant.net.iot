using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Options;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Configuration
{
    [Obsolete("replaced with TriggerEventSource")]
    internal class TriggerConfigurationProvider : ConfigurationProvider
    {
        private const string EventTypesSectionName = nameof(TriggerOptions.Events);

        private readonly IMessagingClient client;

        public TriggerConfigurationProvider(IMessagingClient client) =>
            this.client = client;

        public override void Load() =>
            LoadAsync().ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();

        public async Task LoadAsync()
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
            var references = await client.Request(new TriggerReferencesQuery(), token);
            var tasks = references.Select(x => client.Request(new TriggerQuery(x.RunId), token));
            var runs = await Task.WhenAll(tasks);

            Data = new Dictionary<string, string>();
            foreach (var (activeRun, i) in runs.Where(x => x.IsActive).Select((x, i) => (x, i)))
            {
                Data.Add(Join(ConfigurationNames.TriggerSectionName, EventTypesSectionName, i.ToString(), "name"), activeRun.TriggerEventName);
                foreach (var (pair, j) in activeRun.TriggerEventMask.Select((x, j) => (x, j)))
                    Data.Add(Join(ConfigurationNames.TriggerSectionName, EventTypesSectionName, i.ToString(), "mask", j.ToString(), pair.Key), pair.Value);
            }
        }

        private static string Join(params string[] path) => string.Join(ConfigurationPath.KeyDelimiter, path);
    }
}
