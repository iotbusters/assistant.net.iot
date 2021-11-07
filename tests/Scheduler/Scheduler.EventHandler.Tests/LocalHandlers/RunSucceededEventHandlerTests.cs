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

namespace Assistant.Net.Scheduler.EventHandler.Tests.LocalHandlers
{
    public class RunSucceededEventHandlerTests
    {
        [Test]
        public async Task Handle_RunSucceededEvent_requestsRunQueryAndRunCreateCommand()
        {
            var run = HasRunTrigger(runId: Guid.NewGuid(), nextRunId: null);
            var handler1 = new TestMessageHandler<RunQuery, RunModel>(run);
            var handler2 = new TestMessageHandler<RunCreateCommand, Guid>(Guid.NewGuid());
            var handler3 = new TestMessageHandler<RunUpdateCommand, None>(new None());
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .ReplaceHandler(handler1)
                .ReplaceHandler(handler2)
                .ReplaceHandler(handler3)
                .Build();

            await fixture.Handle(new RunSucceededEvent(run.Id));

            handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(run.Id)});
            handler2.Requests.Should().BeEquivalentTo(new[] {new RunCreateCommand(run.AutomationId)});
            handler3.Requests.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_RunSucceededEvent_requestsRunQueryAndRunUpdateCommand()
        {
            var nextRun = HasRunTrigger(runId: Guid.NewGuid(), nextRunId: null);
            var run = HasRunTrigger(runId: Guid.NewGuid(), nextRun.Id);
            var handler1 = new TestMessageHandler<RunQuery, RunModel>(x =>
            {
                if (x.Id == run.Id) return run;
                if (x.Id == nextRun.Id) return nextRun;
                throw new Exception();
            });
            var handler2 = new TestMessageHandler<RunCreateCommand, Guid>(Guid.NewGuid());
            var handler3 = new TestMessageHandler<RunUpdateCommand, None>(new None());
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .ReplaceHandler(handler1)
                .ReplaceHandler(handler2)
                .ReplaceHandler(handler3)
                .Build();

            await fixture.Handle(new RunSucceededEvent(run.Id));

            handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(run.Id), new RunQuery(run.NextRunId!.Value)});
            handler2.Requests.Should().BeEmpty();
            handler3.Requests.Should().BeEquivalentTo(new[]{new RunUpdateCommand(run.NextRunId!.Value, new RunStatusDto(RunStatus.Started))});
        }

        [Test]
        public async Task Handle_RunSucceededEvent_requestsRunQueryAndTestMessageAndRunUpdateCommand()
        {
            var nextRun = HasRunAction(runId: Guid.NewGuid(), nextRunId: null);
            var run = HasRunTrigger(runId: Guid.NewGuid(), nextRun.Id);
            var handler1 = new TestMessageHandler<RunQuery, RunModel>(x =>
            {
                if (x.Id == run.Id) return run;
                if (x.Id == nextRun.Id) return nextRun;
                throw new Exception();
            });
            var handler2 = new TestMessageHandler<RunCreateCommand, Guid>(Guid.NewGuid());
            var handler3 = new TestMessageHandler<RunUpdateCommand, None>(new None());
            var handler4 = new TestMessageHandler<TestMessage, None>(new None());
            using var fixture = new SchedulerLocalHandlerFixtureBuilder()
                .ReplaceHandler(handler1)
                .ReplaceHandler(handler2)
                .ReplaceHandler(handler3)
                .ReplaceHandler(handler4)
                .Build();

            await fixture.Handle(new RunSucceededEvent(run.Id));

            handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(run.Id), new RunQuery(run.NextRunId!.Value)});
            handler2.Requests.Should().BeEmpty();
            handler3.Requests.Should().BeEquivalentTo(new[] {new RunUpdateCommand(run.NextRunId!.Value, new RunStatusDto(RunStatus.Succeeded))});
            handler4.Requests.Should().HaveCount(1);
        }

        private static RunModel HasRunTrigger(Guid runId, Guid? nextRunId = null) => new(
            runId,
            nextRunId,
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobTriggerModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>()));

        private static RunModel HasRunAction(Guid runId, Guid? nextRunId = null) => new(
            runId,
            nextRunId,
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobActionModel(
                id: Guid.NewGuid(),
                name: "name",
                action: new TestMessage()));
    }
}
