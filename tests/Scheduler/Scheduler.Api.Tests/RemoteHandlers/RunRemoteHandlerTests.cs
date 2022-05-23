using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.RemoteHandlers;

[Timeout(2000)]
public class RunRemoteHandlerTests
{
    [Test]
    public async Task Handle_RunQuery_delegatesQueryAndReturnsRunModel()
    {
        var run = new RunModel(
            id: Guid.NewGuid(),
            nextRunId: Guid.NewGuid(),
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobTriggerEventModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>()));
        var handler = new TestMessageHandler<RunQuery, RunModel>(run);
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceMongoHandler(handler)
            .Build();

        var query = new RunQuery(run.Id);
        var response = await fixture.Handle(query);

        response.Should().BeEquivalentTo(run);
        handler.Request.Should().BeEquivalentTo(query);
    }

    [Test]
    public async Task Handle_RunCreateCommand_delegatesCommandAndReturnsId()
    {
        var runId = Guid.NewGuid();
        var handler = new TestMessageHandler<RunCreateCommand, Guid>(response: runId);
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceMongoHandler(handler)
            .Build();

        var command = new RunCreateCommand(automationId: Guid.NewGuid());
        var id = await fixture.Handle(command);

        id.Should().Be(runId);
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Handle_RunUpdateCommand_delegatesCommand()
    {
        var handler = new TestMessageHandler<RunUpdateCommand, None>(new None());
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceMongoHandler(handler)
            .Build();

        var command = new RunUpdateCommand(id: Guid.NewGuid(), new RunStatusDto(RunStatus.Started, "message"));
        await fixture.Handle(command);

        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Handle_RunDeleteCommand_delegatesCommand()
    {
        var handler = new TestMessageHandler<RunDeleteCommand, None>(new None());
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceMongoHandler(handler)
            .Build();

        var command = new RunDeleteCommand(id: Guid.NewGuid());
        await fixture.Handle(command);

        handler.Request.Should().BeEquivalentTo(command);
    }
}