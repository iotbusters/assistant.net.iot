using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.LocalHandlers
{
    public class JobLocalHandlerTests
    {
        [Test]
        public async Task Handle_JobQuery_returnsJob()
        {
            var job = new JobTriggerModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>());
            var storage = new TestStorage<Guid, JobModel> {{job.Id, job}};
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new JobQuery(job.Id);

            var response = await fixture.Handle(command);

            response.Should().BeEquivalentTo(job);
        }

        [Test]
        public async Task Handle_JobQuery_throwsNotFoundException()
        {
            var job = new JobTriggerModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>());
            var storage = new TestStorage<Guid, JobModel> {{job.Id, job}};
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new JobQuery(UnknownId);

            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }
        
        [Test]
        public async Task Handle_JobCreateCommand_createsJob()
        {
            var storage = new TestStorage<Guid, JobModel>();
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new JobTriggerCreateCommand(
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>());

            var id = await fixture.Handle(command);
            
            var value = await storage.GetOrDefault(id);
            value.Should().BeEquivalentTo(
                new JobTriggerModel(
                    id,
                    command.Name,
                    command.TriggerEventName,
                    command.TriggerEventMask));
        }

        [Test]
        public async Task Handle_JobUpdateCommand_updatesJob()
        {
            var job = new JobTriggerModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>());
            var storage = new TestStorage<Guid, JobModel> {{job.Id, job}};
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new JobTriggerUpdateCommand(job.Id,
                name: "another",
                triggerEventName: nameof(TimerTriggeredEvent),
                triggerEventMask: new Dictionary<string, string> {{"1", "2"}});

            await fixture.Handle(command);

            var ids = await storage.GetKeys().AsEnumerableAsync();
            var id = ids.FirstOrDefault();
            var value = await storage.GetOrDefault(id);
            value.Should().BeEquivalentTo(
                new JobTriggerModel(
                    id,
                    command.Name,
                    command.TriggerEventName,
                    command.TriggerEventMask));
        }

        [Test]
        public async Task Handle_JobUpdateCommand_throwsNotFoundException()
        {
            var job = new JobTriggerModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>());
            var storage = new TestStorage<Guid, JobModel> {{job.Id, job}};
            using var fixture = new SchedulerLocalHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new JobTriggerUpdateCommand(
                UnknownId,
                name: "another",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string> {{"1", "2"}});

            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }

        private static Guid UnknownId => Guid.NewGuid();
    }
}
