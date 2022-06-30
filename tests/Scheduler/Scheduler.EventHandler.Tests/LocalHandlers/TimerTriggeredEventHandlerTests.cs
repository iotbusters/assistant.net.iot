using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.EventHandler.Tests.Fixtures;
using Assistant.Net.Scheduler.EventHandler.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Tests.LocalHandlers;

public class TimerTriggeredEventHandlerTests
{
    [Test]
    public async Task Handle_TimerTriggeredEvent_requestsRunQueryAndRunUpdateCommand()
    {
        var arranged = DateTimeOffset.UtcNow;
        var scheduled = DateTimeOffset.UtcNow;
        var triggered = DateTimeOffset.UtcNow;
        var run = new RunModel(
            id: Guid.NewGuid(),
            nextRunId: Guid.NewGuid(),
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobModel(
                id: Guid.NewGuid(),
                name: "name",
                new JobEventConfigurationDto(
                    eventName: "Event",
                    eventMask: new Dictionary<string, string>())));
        var handler1 = new TestEmptyMessageHandler<RunQuery, RunModel>(run);
        var handler2 = new TestEmptyMessageHandler<RunUpdateCommand, Nothing>(Nothing.Instance);
        using var fixture = new SchedulerLocalEventHandlerFixtureBuilder()
            .ReplaceHandler(handler1)
            .ReplaceHandler(handler2)
            .Build();
        
        await fixture.Handle(new TimerTriggeredEvent(run.Id, arranged, scheduled, triggered));

        handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(run.Id)});
        handler2.Requests.Should().BeEquivalentTo(new[]
        {
            new RunUpdateCommand(
                run.Id,
                new RunStatusDto(RunStatus.Succeeded, $"Timer has triggered the run at {triggered}"))
        });
    }
}
