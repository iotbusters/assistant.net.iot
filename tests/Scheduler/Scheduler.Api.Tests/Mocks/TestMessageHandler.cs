using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Mocks
{
    internal class TestMessageHandler<TRequest, TResponse> : IMessageHandler<TRequest, TResponse>
        where TRequest : class, IMessage<TResponse>
    {
        private readonly Func<TRequest, Task<TResponse>> handle;

        public TestMessageHandler(TResponse response)
            : this(_ => Task.FromResult(response)) { }

        public TestMessageHandler(Func<TRequest, TResponse> handle)
            : this(x => Task.FromResult(handle(x))) { }

        public TestMessageHandler(Func<TRequest, Task<TResponse>> handle) =>
            this.handle = handle;

        public TRequest? Request { get; private set; }

        public async Task<TResponse> Handle(TRequest message)
        {
            Request = message;
            var response = await handle(message);
            return response;
        }
    }
}