﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
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

public sealed class RunSucceededEventHandlerTests
{
    [Test]
    public async Task Handle_RunSucceededEvent_requestsRunQueryAndRunCreateCommand()
    {
        var run = HasRunTrigger(runId: Guid.NewGuid(), nextRunId: null);
        var handler1 = new TestMessageHandler<RunQuery, RunModel>(run);
        var handler2 = new TestMessageHandler<RunCreateCommand, Guid>(Guid.NewGuid());
        using var fixture = new SchedulerLocalEventHandlerFixtureBuilder()
            .ReplaceHandler(handler1)
            .ReplaceHandler(handler2)
            .Build();

        await fixture.Handle(new RunSucceededEvent(run.Id));

        handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(run.Id)});
        handler2.Requests.Should().BeEquivalentTo(new[] {new RunCreateCommand(run.AutomationId)});
    }

    [Test]
    public async Task Handle_RunSucceededEvent_requestsRunQuery()
    {
        var run = HasRunTrigger(runId: Guid.NewGuid(), nextRunId: Guid.NewGuid());
        var handler1 = new TestMessageHandler<RunQuery, RunModel>(x =>
        {
            if (x.Id == run.Id) return run;
            throw new NotFoundException();
        });
        var handler2 = new TestMessageHandler<RunCreateCommand, Guid>(response: Guid.NewGuid());
        var handler3 = new TestMessageHandler<RunStartCommand, Nothing>(response: Nothing.Instance);
        using var fixture = new SchedulerLocalEventHandlerFixtureBuilder()
            .ReplaceHandler(handler1)
            .ReplaceHandler(handler2)
            .ReplaceHandler(handler3)
            .Build();

        await fixture.Handle(new RunSucceededEvent(run.Id));

        handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(run.Id)});
        handler2.Requests.Should().BeEmpty();
        handler3.Requests.Should().HaveCount(1);
    }

    private static RunModel HasRunTrigger(Guid runId, Guid? nextRunId = null) => new(
        runId,
        nextRunId,
        automationId: Guid.NewGuid(),
        jobSnapshot: new JobModel(
            id: Guid.NewGuid(),
            name: "name",
            new JobEventConfigurationDto(
                eventName: "Event",
                eventMask: new Dictionary<string, string>())));
}
