using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Handlers
{
    public class AutomationHandlerTests
    {
        [Test]
        public async Task Handle_AutomationQuery_returnsAutomation()
        {
            var automation = new AutomationModel(Guid.NewGuid(), "name", new[] {new AutomationJobReferenceModel(Guid.NewGuid())});
            var storage = new TestStorage<Guid, AutomationModel> {{automation.Id, automation}};
            using var fixture = new MessageHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new AutomationQuery(automation.Id);

            var response = await fixture.Handle(command);

            response.Should().BeEquivalentTo(automation);
        }

        [Test]
        public async Task Handle_AutomationQuery_throwsNotFoundException()
        {
            var knownId = Guid.NewGuid();
            var unknownId = Guid.NewGuid();
            var automation = new AutomationModel(knownId, "name", new[] {new AutomationJobReferenceModel(Guid.NewGuid())});
            var storage = new TestStorage<Guid, AutomationModel> {{knownId, automation}};
            using var fixture = new MessageHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new AutomationQuery(unknownId);

            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task Handle_AutomationReferencesQuery_returnsAutomationReference()
        {
            var automation = new AutomationModel(Guid.NewGuid(), "name", new[] {new AutomationJobReferenceModel(Guid.NewGuid())});
            var storage = new TestStorage<Guid, AutomationModel> {{automation.Id, automation}};
            using var fixture = new MessageHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new AutomationReferencesQuery();

            var response = await fixture.Handle(command);

            response.Should().BeEquivalentTo(new[] {new AutomationReferenceModel(automation.Id)});
        }

        [Test]
        public async Task Handle_AutomationReferencesQuery_returnsEmpty()
        {
            using var fixture = new MessageHandlerFixtureBuilder().Build();
            var command = new AutomationReferencesQuery();

            var response = await fixture.Handle(command);

            response.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_AutomationCreateCommand_createsAutomation()
        {
            var storage = new TestStorage<Guid, AutomationModel>();
            using var fixture = new MessageHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new AutomationCreateCommand("name", new[] {new JobReferenceDto(Guid.NewGuid())});

            var response = await fixture.Handle(command);

            var ids = await storage.GetKeys().AsEnumerableAsync();
            var id = ids.FirstOrDefault();
            id.Should().Be(response);

            var value = await storage.GetOrDefault(response);
            value.Should().BeEquivalentTo(
                new AutomationModel(id, command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id))));
        }

        [Test]
        public async Task Handle_AutomationUpdateCommand_updatesAutomation()
        {
            var automation = new AutomationModel(Guid.NewGuid(), "name", new[] {new AutomationJobReferenceModel(Guid.NewGuid())});
            var storage = new TestStorage<Guid, AutomationModel> {{automation.Id, automation}};
            using var fixture = new MessageHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new AutomationUpdateCommand(automation.Id, "another", new[] {new JobReferenceDto(Guid.NewGuid())});

            await fixture.Handle(command);

            var ids = await storage.GetKeys().AsEnumerableAsync();
            var id = ids.FirstOrDefault();
            var value = await storage.GetOrDefault(id);
            value.Should().BeEquivalentTo(
                new AutomationModel(id, command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id))));
        }

        [Test]
        public async Task Handle_AutomationUpdateCommand_throwsNotFoundException()
        {
            var knownId = Guid.NewGuid();
            var unknownId = Guid.NewGuid();
            var automation = new AutomationModel(knownId, "name", new[] {new AutomationJobReferenceModel(Guid.NewGuid())});
            var storage = new TestStorage<Guid, AutomationModel> {{knownId, automation}};
            using var fixture = new MessageHandlerFixtureBuilder().AddStorage(storage).Build();
            var command = new AutomationUpdateCommand(unknownId, "another", new[] {new JobReferenceDto(Guid.NewGuid())});

            await fixture.Awaiting(x => x.Handle(command))
                .Should().ThrowAsync<NotFoundException>();
        }
    }
}
