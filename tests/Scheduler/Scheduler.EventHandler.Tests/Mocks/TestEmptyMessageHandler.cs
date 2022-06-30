using Assistant.Net.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Tests.Mocks;

internal class TestEmptyMessageHandler<TRequest, TResponse> : IMessageHandler<TRequest, TResponse>
    where TRequest : class, IMessage<TResponse>
{
    private readonly Func<TRequest, Task<TResponse>> handle;

    public TestEmptyMessageHandler(TResponse response)
        : this(_ => Task.FromResult(response)) { }

    public TestEmptyMessageHandler(Func<TRequest, TResponse> handle)
        : this(x => Task.FromResult(handle(x))) { }

    public TestEmptyMessageHandler(Func<TRequest, Task<TResponse>> handle) =>
        this.handle = handle;

    public IList<TRequest> Requests { get; private set; } = new List<TRequest>();

    public async Task<TResponse> Handle(TRequest message, CancellationToken token)
    {
        Requests.Add(message);
        var response = await handle(message);
        return response;
    }
}