using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Handlers
{
    internal class RunFailedEventHandler : IMessageHandler<RunFailedEvent>
    {
        private readonly ILogger logger;
        private readonly IMessagingClient client;

        public RunFailedEventHandler(ILogger<RunFailedEventHandler> logger, IMessagingClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public Task Handle(RunFailedEvent @event, CancellationToken token)
        {
            // note: automation is implicitly blocked.
            // todo: resolve run failure and automation blocking.
            //var run = await client.Request(new RunQuery(message.RunId), token);
            return Task.CompletedTask;
        }
    }
}
