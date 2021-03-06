using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Tests.Fixtures;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Tests.RemoteHandlers;

public class TriggerEventHandlerTests
{
    [Test]
    public async Task Request_triggersHandler_receivedTestEvent()
    {
        var runId = Guid.NewGuid();
        var handler01 = TriggerReferencesQueryHandler(runId);
        var handler02 = TriggerQueryHandler();
        var handler1 = TriggerQueryHandler();
        var handler2 = RunUpdateCommandHandler();

        using var fixture = new SchedulerRemoteTriggerHandlerFixtureBuilder()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceRemoteHandler(handler01)
            .ReplaceRemoteHandler(handler02)
            .ReplaceRemoteServerHandler(handler1)
            .ReplaceRemoteServerHandler(handler2)
            .AddRegistrationOnly<TestEvent>()
            .Build();

        await fixture.Client.Publish(new TestEvent());
        await Task.Delay(TimeSpan.FromSeconds(0.1));

        handler1.Requests.Should().BeEquivalentTo(new[] {new TriggerQuery(runId)});
        var succeeded = new RunStatusDto(RunStatus.Succeeded, "The event has been raised.");
        handler2.Requests.Should().BeEquivalentTo(new[] {new RunUpdateCommand(runId, succeeded)});
    }

    [Test]
    public async Task Request_doesNotTriggerHandler_receivedUnknownTestEvent()
    {
        var runId = Guid.NewGuid();
        var handler01 = TriggerReferencesQueryHandler(runId);
        var handler02 = TriggerQueryHandler();
        var handler1 = TriggerQueryHandler();
        var handler2 = RunUpdateCommandHandler();
        using var fixture = new SchedulerRemoteTriggerHandlerFixtureBuilder()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceRemoteHandler(handler01)
            .ReplaceRemoteHandler(handler02)
            .ReplaceRemoteServerHandler(handler1)
            .ReplaceRemoteServerHandler(handler2)
            .AddRegistrationOnly<TestEvent<string>>()
            .Build();

        await fixture.Client.Publish(new TestEvent<string>(""));
        await Task.Delay(TimeSpan.FromSeconds(0.1));

        handler1.Requests.Should().BeEmpty();
        handler2.Requests.Should().BeEmpty();
    }

    private TestMessageHandler<TriggerReferencesQuery, IEnumerable<TriggerReferenceModel>> TriggerReferencesQueryHandler(params Guid[] runIds) =>
        new(runIds.Select(x => new TriggerReferenceModel(x)).ToArray());

    private TestMessageHandler<TriggerQuery, TriggerModel> TriggerQueryHandler() =>
        new(query =>
        {
            var triggerEventMask = new Dictionary<string, string>();
            return new TriggerModel(query.RunId, isActive: true, triggerEventName: nameof(TestEvent), triggerEventMask);
        });

    private TestMessageHandler<RunUpdateCommand, Nothing> RunUpdateCommandHandler() => new(Nothing.Instance);
}
