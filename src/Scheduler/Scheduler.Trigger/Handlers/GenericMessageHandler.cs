using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Handlers
{
    internal class GenericMessageHandler<TMessage> : IMessageHandler<TMessage> where TMessage : IMessage
    {
        private readonly ILogger<GenericMessageHandler<TMessage>> logger;

        public GenericMessageHandler(ILogger<GenericMessageHandler<TMessage>> logger) =>
            this.logger = logger;

        public Task Handle(TMessage message, CancellationToken token)
        {
            logger.LogInformation("Received {@Message}.");

            return Task.CompletedTask;
        }
    }
}
