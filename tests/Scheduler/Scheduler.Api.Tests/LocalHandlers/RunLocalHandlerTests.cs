using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.LocalHandlers;

public sealed class RunLocalHandlerTests
{
    [Test]
    public async Task Handle_RunQuery_returnsRun()
    {
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
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(run.Id, run).Build();

        var command = new RunQuery(run.Id);
        var response = await fixture.Handle(command);

        response.Should().BeEquivalentTo(run);
    }

    [Test]
    public async Task Handle_RunQuery_throwsNotFoundException()
    {
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Build();

        var command = new RunQuery(id: Guid.NewGuid());
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_RunCreateCommand_createsRun()
    {
        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        var automation = new AutomationModel(id: Guid.NewGuid(), name: "name", jobs: new[] {new AutomationJobReferenceModel(job.Id)});
        var triggerHandler = new TestMessageHandler<TriggerCreatedEvent, Nothing>(Nothing.Instance);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder()
            .Store(job.Id, job)
            .Store(automation.Id, automation)
            .ReplaceHandler(triggerHandler)
            .Build();

        var command = new RunCreateCommand(automation.Id);
        var runId = await fixture.Handle(command);

        var createdRun = await fixture.GetOrDefault<Guid, RunModel>(runId);
        createdRun.Should().BeEquivalentTo(
            new RunModel(runId, nextRunId: null, automationId: automation.Id, jobSnapshot: job)
                .WithStatus(RunStatus.Started));
        var createdTrigger = await fixture.GetOrDefault<Guid, TriggerModel>(runId);
        createdTrigger.Should().BeEquivalentTo(
            new TriggerModel(runId, isActive: true, configuration.EventName, configuration.EventMask));
        triggerHandler.Request.Should().BeEquivalentTo(new TriggerCreatedEvent(runId));
    }

    [Test]
    public async Task Handle_RunCreateCommand_throwsNotFoundException_noAutomation()
    {
        var runStorage = new TestStorage<Guid, RunModel>();
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder()
            .ReplaceStorage(runStorage)
            .Build();

        var command = new RunCreateCommand(automationId: Guid.NewGuid());
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_RunCreateCommand_throwsNotFoundException_noJob()
    {
        var automation = new AutomationModel(
            id: Guid.NewGuid(),
            name: "name",
            jobs: new[] {new AutomationJobReferenceModel(id: Guid.NewGuid())});
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(automation.Id, automation).Build();

        var command = new RunCreateCommand(automation.Id);
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_RunSucceededEvent_updatesRunAndPublishesSucceededEvent()
    {
        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var jobSnapshot = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        var run = new RunModel(
            id: Guid.NewGuid(),
            nextRunId: Guid.NewGuid(),
            automationId: Guid.NewGuid(),
            jobSnapshot: jobSnapshot).WithStatus(RunStatus.Started);
        var trigger = new TriggerModel(
            run.Id,
            isActive: true,
            triggerEventName: "Event",
            triggerEventMask: new Dictionary<string, string>());
        var runHandler = new TestMessageHandler<RunSucceededEvent, Nothing>(Nothing.Instance);
        var triggerHandler = new TestMessageHandler<TriggerDeactivatedEvent, Nothing>(Nothing.Instance);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder()
            .Store(run.Id, run)
            .Store(run.Id, trigger)
            .ReplaceHandler(runHandler)
            .ReplaceHandler(triggerHandler)
            .Build();

        var command = new RunSucceedCommand(run.Id);
        await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, RunModel>(run.Id);
        value.Should().BeEquivalentTo(run.WithStatus(RunStatus.Succeeded));
        runHandler.Request.Should().BeEquivalentTo(new RunSucceededEvent(run.Id));
    }

    [Test]
    public async Task Handle_RunSucceededEvent_updatesRunAndPublishesFailedEvent()
    {
        var run = new RunModel(
            id: Guid.NewGuid(),
            nextRunId: Guid.NewGuid(),
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobModel(
                id: Guid.NewGuid(),
                name: "name",
                new JobEventConfigurationDto(
                    eventName: "Event",
                    eventMask: new Dictionary<string, string>())))
            .WithStatus(RunStatus.Started);
        var trigger = new TriggerModel(
            run.Id,
            isActive: true,
            triggerEventName: "Event",
            triggerEventMask: new Dictionary<string, string>());

        var storage = new TestStorage<Guid, RunModel> {{run.Id, run}};
        var runHandler = new TestMessageHandler<RunFailedEvent, Nothing>(Nothing.Instance);
        var triggerHandler = new TestMessageHandler<TriggerDeactivatedEvent, Nothing>(Nothing.Instance);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder()
            .ReplaceStorage(storage)
            .ReplaceStorage(new TestStorage<Guid, TriggerModel> {{run.Id, trigger}})
            .ReplaceHandler(runHandler)
            .ReplaceHandler(triggerHandler)
            .Build();

        var command = new RunFailCommand(run.Id);
        await fixture.Handle(command);

        var value = await storage.GetOrDefault(run.Id);
        value.Should().BeEquivalentTo(run.WithStatus(RunStatus.Failed));
        runHandler.Request.Should().BeEquivalentTo(new RunFailedEvent(run.Id));
        triggerHandler.Request.Should().BeEquivalentTo(new TriggerDeactivatedEvent(run.Id));
    }

    [Test]
    public async Task Handle_RunSucceededEvent_updatesRunAndRequestsTriggerCreateCommand()
    {
        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var jobSnapshot = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        var run = new RunModel(id: Guid.NewGuid(), nextRunId: Guid.NewGuid(), automationId: Guid.NewGuid(), jobSnapshot: jobSnapshot);
        var handler = new TestMessageHandler<TriggerCreateCommand, Guid>(Guid.NewGuid());
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().ReplaceHandler(handler).Store(run.Id, run).Build();

        var command = new RunStartCommand(run.Id);
        await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, RunModel>(run.Id);
        value.Should().BeEquivalentTo(run.WithStatus(RunStatus.Started));
        handler.Request.Should().BeEquivalentTo(new TriggerCreateCommand(run.Id));
    }

    [Test]
    public async Task Handle_RunSucceededEvent_throwsNotFoundException()
    {
        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var jobSnapshot = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        var run = new RunModel(id: Guid.NewGuid(), nextRunId: Guid.NewGuid(), automationId: Guid.NewGuid(), jobSnapshot: jobSnapshot);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(run.Id, run).Build();

        var command = new RunSucceedCommand(id: Guid.NewGuid());
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }
}
