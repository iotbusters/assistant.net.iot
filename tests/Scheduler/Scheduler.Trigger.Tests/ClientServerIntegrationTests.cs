using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Scheduler.Trigger.Tests.Fixtures;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Tests;

[Timeout(2000)]
public class ClientServerIntegrationTests
{
    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_noRegisteredHandler()
    {
        var handler2 = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .AddHandler(handler2) // just to pass validation.
            .Build();

        await fixture.Client.Awaiting(x => x.RequestObject(new TestMessage1("1")))
            .Should().ThrowAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{nameof(TestMessage1)}' wasn't registered.");
    }

    [Test]
    public async Task RequestObject_returnsResponse_registeredHandler()
    {
        var handler1 = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));

        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .AddHandler(handler1)
            .Build();

        var response = await fixture.Client.RequestObject(new TestMessage1("1"));
        response.Should().BeEquivalentTo(new TestResponse("1"));
    }

    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_noLongerRegisteredHandler1()
    {
        var handler1 = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));
        var handler2 = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .AddHandler(handler1)
            .Build();

        fixture.ReplaceHandlers(handler2);

        await fixture.Client.Awaiting(x => x.RequestObject(new TestMessage1("1")))
            .Should().ThrowAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{nameof(TestMessage1)}' wasn't registered.");
    }

    [Test]
    public async Task RequestObject_returnsResponse_laterRegisteredHandler2()
    {
        var handler1 = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));
        var handler2 = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .AddHandler(handler1)
            .Build();

        fixture.ReplaceHandlers(handler2);

        var response = await fixture.Client.RequestObject(new TestMessage2("2"));
        response.Should().BeEquivalentTo(new TestResponse("2"));
    }

    [Test]
    public async Task RequestObject_returnsResponse_multipleRegisteredHandlers()
    {
        var handler1 = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));
        var handler2 = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .AddHandler(handler1)
            .AddHandler(handler2)
            .Build();

        var response1 = await fixture.Client.RequestObject(new TestMessage1("1"));
        response1.Should().BeEquivalentTo(new TestResponse("1"));
        var response2 = await fixture.Client.RequestObject(new TestMessage2("2"));
        response2.Should().BeEquivalentTo(new TestResponse("2"));
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var provider = new ServiceCollection()
            .ConfigureMongoOptions("", o => o.Connection(SetupMongo.ConnectionString).Database(SetupMongo.Database))
            .AddMongoClientFactory()
            .BuildServiceProvider();

        string pingContent;
        var mongoClient = provider.GetRequiredService<IMongoClientFactory>().CreateClient("");
        try
        {
            var ping = await mongoClient.GetDatabase("db").RunCommandAsync(
                (Command<BsonDocument>)"{ping:1}",
                ReadPreference.Nearest,
                new CancellationTokenSource(200).Token);
            pingContent = ping.ToString();
        }
        catch
        {
            pingContent = string.Empty;
        }
        if (!pingContent.Contains("ok"))
            Assert.Ignore($"The tests require mongodb instance at {SetupMongo.ConnectionString}.");
    }
}
