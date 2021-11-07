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

namespace Assistant.Net.Scheduler.Api.Tests.LocalHandlers
{
    public class RunLocalHandlerTests
    {
        [Test]
        public async Task Handle_RunQuery_returnsRun()
        {
            var run = new RunModel(
                id: Guid.NewGuid(),
                nextRunId: Guid.NewGuid(),
                automationId: Guid.NewGuid(),
                jobSnapshot: new JobTriggerModel(
                    id: Guid.NewGuid(),
                    name: "name",
                    triggerEventName: "Event",
                    triggerEventMask: new Dictionary<string, string>()));
            var storage = new TestStorage<Guid, RunModel> {{run.Id, run}};
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().AddStorage(storage).Build();

            var command = new RunQuery(run.Id);
            var response = await fixture.Handle(command);

            response.Should().BeEquivalentTo(run);
        }

        [Test]
        public async Task Handle_RunQuery_throwsNotFoundException()
        {
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().Build();

            var command = new RunQuery(UnknownId);
            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task Handle_RunCreateCommand_createsRun()
        {
            var job = new JobTriggerModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>());
            var automation = new AutomationModel(
                id: Guid.NewGuid(),
                name: "name",
                jobs: new[] {new AutomationJobReferenceModel(job.Id)});
            var runStorage = new TestStorage<Guid, RunModel>();
            var triggerStorage = new TestStorage<Guid, TriggerModel>();
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .AddStorage(runStorage)
                .AddStorage(triggerStorage)
                .AddStorage(new TestStorage<Guid, JobModel> {{job.Id, job}})
                .AddStorage(new TestStorage<Guid, AutomationModel> {{automation.Id, automation}})
                .Build();

            var command = new RunCreateCommand(automation.Id);
            var runId = await fixture.Handle(command);

            var createdRun = await runStorage.GetOrDefault(runId);
            createdRun.Should().BeEquivalentTo(
                new RunModel(runId, nextRunId: null, automationId: automation.Id, jobSnapshot: job)
                    .WithStatus(new RunStatusDto(RunStatus.Started)));
            var createdTrigger = await triggerStorage.GetOrDefault(runId);
            createdTrigger.Should().BeEquivalentTo(
                new TriggerModel(runId, isActive: true, job.TriggerEventName, job.TriggerEventMask));
        }

        [Test]
        public async Task Handle_RunCreateCommand_throwsNotFoundException_noAutomation()
        {
            var runStorage = new TestStorage<Guid, RunModel>();
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .AddStorage(runStorage)
                .Build();

            var command = new RunCreateCommand(UnknownId);
            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task Handle_RunCreateCommand_throwsNotFoundException_noJob()
        {
            var automation = new AutomationModel(
                id: Guid.NewGuid(),
                name: "name",
                jobs: new[] {new AutomationJobReferenceModel(UnknownId)});
            var runStorage = new TestStorage<Guid, RunModel>();
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .AddStorage(runStorage)
                .AddStorage(new TestStorage<Guid, AutomationModel> {{automation.Id, automation}})
                .Build();

            var command = new RunCreateCommand(automation.Id);
            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task Handle_RunUpdateCommand_updatesRunAndPublishesSucceededEvent()
        {
            var run = new RunModel(
                id: Guid.NewGuid(),
                nextRunId: Guid.NewGuid(),
                automationId: Guid.NewGuid(),
                jobSnapshot: new JobTriggerModel(
                    id: Guid.NewGuid(),
                    name: "name",
                    triggerEventName: "Event",
                    triggerEventMask: new Dictionary<string, string>()));
            var storage = new TestStorage<Guid, RunModel> {{run.Id, run}};
            var handler = new TestMessageHandler<RunSucceededEvent, None>(new None());
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .ReplaceHandler(handler)
                .AddStorage(storage)
                .Build();

            var command = new RunUpdateCommand(run.Id, new RunStatusDto(RunStatus.Succeeded, "OK"));
            await fixture.Handle(command);

            var value = await storage.GetOrDefault(run.Id);
            value.Should().BeEquivalentTo(run.WithStatus(new RunStatusDto(RunStatus.Succeeded, "OK")));
            handler.Request.Should().BeEquivalentTo(new RunSucceededEvent(run.Id));
        }

        [Test]
        public async Task Handle_RunUpdateCommand_updatesRunAndPublishesFailedEvent()
        {
            var run = new RunModel(
                id: Guid.NewGuid(),
                nextRunId: Guid.NewGuid(),
                automationId: Guid.NewGuid(),
                jobSnapshot: new JobTriggerModel(
                    id: Guid.NewGuid(),
                    name: "name",
                    triggerEventName: "Event",
                    triggerEventMask: new Dictionary<string, string>()));
            var storage = new TestStorage<Guid, RunModel> {{run.Id, run}};
            var handler = new TestMessageHandler<RunFailedEvent, None>(new None());
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .ReplaceHandler(handler)
                .AddStorage(storage)
                .Build();

            var command = new RunUpdateCommand(run.Id, new RunStatusDto(RunStatus.Failed, "OK"));
            await fixture.Handle(command);

            var value = await storage.GetOrDefault(run.Id);
            value.Should().BeEquivalentTo(run.WithStatus(new RunStatusDto(RunStatus.Failed, "OK")));
            handler.Request.Should().BeEquivalentTo(new RunFailedEvent(run.Id));
        }

        [Test]
        public async Task Handle_RunUpdateCommand_updatesRunAndRequestsTriggerCreateCommand()
        {
            var run = new RunModel(
                id: Guid.NewGuid(),
                nextRunId: Guid.NewGuid(),
                automationId: Guid.NewGuid(),
                jobSnapshot: new JobTriggerModel(
                    id: Guid.NewGuid(),
                    name: "name",
                    triggerEventName: "Event",
                    triggerEventMask: new Dictionary<string, string>()));
            var storage = new TestStorage<Guid, RunModel> { { run.Id, run } };
            var handler = new TestMessageHandler<TriggerCreateCommand, Guid>(Guid.NewGuid());
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .ReplaceHandler(handler)
                .AddStorage(storage)
                .Build();

            var command = new RunUpdateCommand(run.Id, new RunStatusDto(RunStatus.Started, "OK"));
            await fixture.Handle(command);

            var value = await storage.GetOrDefault(run.Id);
            value.Should().BeEquivalentTo(run.WithStatus(new RunStatusDto(RunStatus.Started, "OK")));
            handler.Request.Should().BeEquivalentTo(new TriggerCreateCommand(run.Id));
        }

        [Test]
        public async Task Handle_RunUpdateCommand_throwsNotFoundException()
        {
            var run = new RunModel(
                id: Guid.NewGuid(),
                nextRunId: Guid.NewGuid(),
                automationId: Guid.NewGuid(),
                jobSnapshot: new JobTriggerModel(
                    id: Guid.NewGuid(),
                    name: "name",
                    triggerEventName: "Event",
                    triggerEventMask: new Dictionary<string, string>()));
            var storage = new TestStorage<Guid, RunModel> {{run.Id, run}};
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().AddStorage(storage).Build();

            var command = new RunUpdateCommand(UnknownId, new RunStatusDto(RunStatus.Succeeded, "OK"));
            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }

        private static Guid UnknownId => Guid.NewGuid();
    }
}
