using Assistant.Net.Messaging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class SchedulerLocalHandlerFixture : IDisposable
    {
        private readonly ServiceProvider provider;

        public SchedulerLocalHandlerFixture(ServiceProvider provider) =>
            this.provider = provider;

        public async Task<TResponse> Handle<TResponse>(IMessage<TResponse> request)
        {
            var handler = provider.GetService<IMessagingClient>();
            handler.Should().NotBeNull();

            return await handler!.Request(request);
        }

        public virtual void Dispose() =>
            provider.Dispose();
    }
}
