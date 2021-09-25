using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Scheduler.Contracts.Queries;
using System;

namespace Assistant.Net.Scheduler.EventHandler.Internal
{
    internal class RemoteWebMessageHandlersConfiguration : IMessageConfiguration
    {
        /// <exception cref="InvalidOperationException" />
        public void Configure(MessagingClientBuilder builder) => builder
            .AddWeb<AutomationReferencesQuery>()
            .AddWeb<AutomationQuery>()
            .AddWeb<JobQuery>()
        ;
    }
}
