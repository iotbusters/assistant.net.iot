using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class LocalCommandHandlersConfiguration : IMessageConfiguration
    {
        public void Configure(MessagingClientBuilder builder) => builder
            .AddLocal<AutomationQueryHandler>()
            .AddLocal<AutomationReferencesQueryHandler>()
            .AddLocal<AutomationCreateCommandHandler>()
            .AddLocal<AutomationUpdateCommandHandler>()
            .AddLocal<AutomationDeleteCommandHandler>()
            .AddLocal<JobQueryHandler>()
            .AddLocal<JobCreateCommandHandler>()
            .AddLocal<JobUpdateCommandHandler>()
            .AddLocal<JobDeleteCommandHandler>();
    }
}