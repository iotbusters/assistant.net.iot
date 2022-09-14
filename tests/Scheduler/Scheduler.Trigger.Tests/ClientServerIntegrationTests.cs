//using Assistant.Net.Messaging.Abstractions;
//using Assistant.Net.Messaging.Exceptions;
//using Assistant.Net.Scheduler.Trigger.Tests.Fixtures;
//using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
//using FluentAssertions;
//using NUnit.Framework;
//using System.Threading.Tasks;

//namespace Assistant.Net.Scheduler.Trigger.Tests;

//[Timeout(2000)]
//public sealed class ClientServerIntegrationTests
//{
//    [Test]
//    public async Task Request_throwsMessageNotRegisteredException_noRegisteredHandler()
//    {
//        var handler = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
//        using var fixture = new SchedulerRemoteTriggerHandlerFixtureBuilder()
//            .UseSqlite(SetupSqlite.ConnectionString)
//            .ReplaceHandler(handler) // just to pass validation.
//            .Build();

//        await fixture.Client.Awaiting(x => x.Request(new TestMessage1("1")))
//            .Should().ThrowAsync<MessageNotRegisteredException>()
//            .WithMessage($"Message '{nameof(TestMessage1)}' wasn't registered.");
//    }

//    [Test]
//    public async Task Request_returnsResponse_registeredHandler()
//    {
//        var handler = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));

//        using var fixture = new SchedulerRemoteTriggerHandlerFixtureBuilder()
//            .UseSqlite(SetupSqlite.ConnectionString)
//            .ReplaceHandler(handler)
//            .Build();

//        var response = await fixture.Client.Request(new TestMessage1("1"));
//        response.Should().BeEquivalentTo(new TestResponse("1"));
//    }

//    [Test]
//    public async Task Request_throwsMessageNotRegisteredException_noLongerRegisteredHandler1()
//    {
//        var handler1 = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));
//        var handler2 = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
//        using var fixture = new SchedulerRemoteTriggerHandlerFixtureBuilder()
//            .UseSqlite(SetupSqlite.ConnectionString)
//            .ReplaceHandler(handler1)
//            .Build();

//        fixture.ReplaceHandlers(handler2);

//        await fixture.Client.Awaiting(x => x.Request(new TestMessage1("1")))
//            .Should().ThrowAsync<MessageNotRegisteredException>()
//            .WithMessage($"Message '{nameof(TestMessage1)}' wasn't registered.");
//    }

//    [Test]
//    public async Task Request_returnsResponse_laterRegisteredHandler2()
//    {
//        var handler1 = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));
//        var handler2 = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
//        using var fixture = new SchedulerRemoteTriggerHandlerFixtureBuilder()
//            .UseSqlite(SetupSqlite.ConnectionString)
//            .ReplaceHandler(handler1)
//            .Build();

//        fixture.ReplaceHandlers(handler2);

//        var response = await fixture.Client.Request(new TestMessage2("2"));
//        response.Should().BeEquivalentTo(new TestResponse("2"));
//    }

//    [Test]
//    public async Task Request_returnsResponse_multipleRegisteredHandlers()
//    {
//        var handler1 = new TestMessageHandler<TestMessage1, TestResponse>(x => new TestResponse(x.Text));
//        var handler2 = new TestMessageHandler<TestMessage2, TestResponse>(x => new TestResponse(x.Text));
//        using var fixture = new SchedulerRemoteTriggerHandlerFixtureBuilder()
//            .UseSqlite(SetupSqlite.ConnectionString)
//            .ReplaceHandler(handler1)
//            .ReplaceHandler(handler2)
//            .Build();

//        var response1 = await fixture.Client.Request(new TestMessage1("1"));
//        response1.Should().BeEquivalentTo(new TestResponse("1"));
//        var response2 = await fixture.Client.Request(new TestMessage2("2"));
//        response2.Should().BeEquivalentTo(new TestResponse("2"));
//    }
//}
