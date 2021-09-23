using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class MessageHandlerFixture : IDisposable
    {
        public readonly ServiceProvider provider;

        public MessageHandlerFixture(ServiceProvider provider) =>
            this.provider = provider;

        public async Task<TResponse> Handle<TResponse>(IMessage<TResponse> request)
        {
            var handler = provider.GetService<IMessagingClient>();
            handler.Should().NotBeNull();

            return await handler!.SendAs(request);
        }

        public void Dispose() =>
            provider.Dispose();
    }
}
