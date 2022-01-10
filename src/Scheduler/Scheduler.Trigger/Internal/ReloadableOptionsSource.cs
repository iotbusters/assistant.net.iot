using Assistant.Net.Abstractions;
using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Scheduler.Trigger.Internal
{
    internal class ReloadableOptionsSource : ConfigureOptionsSourceBase<MessagingClientOptions>
    {
        private readonly ILogger<Startup> logger;
        private readonly IMessageHandlerFactory factory;

        public ReloadableOptionsSource(
            ILogger<Startup> logger,
            IMessageHandlerFactory factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        public HashSet<Type> MessageTypes { get; private set; } = new();

        public override void Configure(MessagingClientOptions options)
        {
            options.Handlers.Clear();
            foreach (var messageType in MessageTypes)
            {
                var handler = factory.Create(messageType);
                options.AddHandler(handler);
            }
        }

        public void Reload(IEnumerable<Type> messageTypes)
        {
            var types = messageTypes.ToHashSet();

            var unsupportedTypes = types.Where(x => !x.IsAssignableTo(typeof(IMessage<None>))).ToArray();
            if (unsupportedTypes.Any())
            {
                foreach (var unsupportedType in unsupportedTypes)
                    types.Remove(unsupportedType);

                logger.LogDebug("Found unsupported {MessageTypes} expecting a response.", (object)unsupportedTypes);
            }

            MessageTypes = types;

            logger.LogDebug("Reload {MessageTypes}.", types);

            Reload();
        }
    }
}
