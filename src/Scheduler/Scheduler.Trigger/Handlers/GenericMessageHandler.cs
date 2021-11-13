using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Handlers
{
    internal class GenericMessageHandler<TMessage, TResponse> : IMessageHandler<TMessage, TResponse> where TMessage : IMessage<TResponse>
    {
        public Task<TResponse> Handle(TMessage message, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}
