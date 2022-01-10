using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Tests.Fixtures;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Tests;

[Timeout(2000)]
public class ClientServerIntegrationTests
{
    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_noHandler()
    {
        var handler = new TestMessageHandler<TriggerReferencesQuery, TriggerReferenceModel[]>(Array.Empty<TriggerReferenceModel>());
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceHandler(handler) // just to pass validation.
            .Build();

        await fixture.Client.Awaiting(x => x.RequestObject(new TestMessage("1")))
            .Should().ThrowAsync<MessageNotRegisteredException>()
            .WithMessage("Message 'TestMessage' wasn't registered.");
    }

    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_noLongerHandler()
    {
        var handler = new TestMessageHandler<TestMessage, TestResponse>(x => new TestResponse(x.Text));
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceHandler(handler)
            .Build();

        //var o0 = fixture.Service<IOptionsMonitor<MongoHandlingOptions>>();
        //var o01 = o0.CurrentValue;
        //var o1 = fixture.Service<IOptionsMonitor<MessagingClientOptions>>();
        //var o11 = o1.Get("mongo.server");

        fixture.RemoveHandler(handler);
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();
        //await Task.Yield();

        //var o2 = fixture.Service<IOptionsMonitor<MongoHandlingOptions>>();
        //var o21 = o0.CurrentValue;
        //var o3 = fixture.Service<IOptionsMonitor<MessagingClientOptions>>();
        //var o31 = o1.Get("mongo.server");

        //var a = await fixture.Client.RequestObject(new TestMessage("1"));

        await fixture.Client.Awaiting(x => x.RequestObject(new TestMessage("1")))
            .Should().ThrowAsync<MessageNotRegisteredException>()
            .WithMessage("Message 'TestMessage' wasn't registered.");
    }

    [Test]
    public async Task RequestObject_returnsResponse()
    {
        var handler = new TestMessageHandler<TestMessage, TestResponse>(x => new TestResponse(x.Text));

        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            //.AddInterceptor(new ErrorHandlingInterceptor())
            .ReplaceHandler(handler)
            .Build();

        var response = await fixture.Client.RequestObject(new TestMessage("1"));

        response.Should().BeEquivalentTo(new TestResponse("1"));
    }

    //public class ErrorHandlingInterceptor : ErrorHandlingInterceptor<IMessage<object>, object>, IMessageInterceptor
    //{
    //}

    ///// <summary>
    /////     Global error handling interceptor.
    ///// </summary>
    ///// <remarks>
    /////     The interceptor depends on <see cref="MessagingClientOptions.ExposedExceptions"/>
    ///// </remarks>
    //public class ErrorHandlingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    //    where TMessage : IMessage<TResponse>
    //{
    //    /// <inheritdoc/>
    //    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
    //    {
    //        try
    //        {
    //            return await next(message, token);
    //        }
    //        catch (Exception ex)
    //        {
    //            throw;
    //        }

    //    }
    //}
}
