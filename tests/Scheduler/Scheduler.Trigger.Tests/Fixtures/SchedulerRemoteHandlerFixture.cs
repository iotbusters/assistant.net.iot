using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures
{
    public class SchedulerRemoteHandlerFixture : IDisposable
    {
        private readonly TestTriggerQueriesHandler triggerHandler;
        private readonly TestMessageHandlerFactory factory;
        private readonly ServiceProvider provider;
        private readonly IHost host;

        public SchedulerRemoteHandlerFixture(
            TestTriggerQueriesHandler triggerHandler,
            TestMessageHandlerFactory factory,
            ServiceProvider provider,
            IHost host)
        {
            this.triggerHandler = triggerHandler;
            this.factory = factory;
            this.provider = provider;
            this.host = host;
        }

        public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

        public SchedulerRemoteHandlerFixture RemoveHandler(object handler)
        {
            var messageTypes = handler.GetType().GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
            if (!messageTypes.Any())
                throw new ArgumentException("Invalid message handler type.", nameof(handler));

            foreach (var messageType in messageTypes)
            {
                factory.Remove(messageType);
                triggerHandler.Remove(messageType);
            }

            return this;
        }

        //public T Service<T>() where T : class => provider.GetRequiredService<T>();

        public void Dispose()
        {
            host.Dispose();
            provider.Dispose();
        }
    }
}
