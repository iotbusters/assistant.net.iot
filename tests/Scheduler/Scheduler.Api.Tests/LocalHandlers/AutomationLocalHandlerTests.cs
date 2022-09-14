using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.LocalHandlers;

public sealed class AutomationLocalHandlerTests
{
    [Test]
    public async Task Handle_AutomationQuery_returnsAutomation()
    {
        var automation = new AutomationModel(id: Guid.NewGuid(), "name", jobs: new[] {new AutomationJobReferenceModel(id: Guid.NewGuid())});
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(automation.Id, automation).Build();

        var command = new AutomationQuery(automation.Id);
        var response = await fixture.Handle(command);

        response.Should().BeEquivalentTo(automation);
    }

    [Test]
    public async Task Handle_AutomationQuery_throwsNotFoundException()
    {
        var automation = new AutomationModel(id: Guid.NewGuid(), "name", jobs: new[] {new AutomationJobReferenceModel(id: Guid.NewGuid())});
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(automation.Id, automation).Build();

        var command = new AutomationQuery(id: Guid.NewGuid());
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_AutomationReferencesQuery_returnsAutomationReference()
    {
        var automation = new AutomationModel(id: Guid.NewGuid(), "name", jobs: new[] {new AutomationJobReferenceModel(id: Guid.NewGuid())});
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(automation.Id, automation).Build();

        var command = new AutomationReferencesQuery();
        var response = await fixture.Handle(command);

        response.Should().BeEquivalentTo(new[] {new AutomationReferenceModel(automation.Id)});
    }

    [Test]
    public async Task Handle_AutomationReferencesQuery_returnsEmpty()
    {
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Build();

        var command = new AutomationReferencesQuery();
        var response = await fixture.Handle(command);

        response.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_AutomationCreateCommand_createsAutomation()
    {
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Build();

        var command = new AutomationCreateCommand("name", new[] {new JobReferenceDto(id: Guid.NewGuid())});
        var id = await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, AutomationModel>(id);
        value.Should().BeEquivalentTo(
            new AutomationModel(id, command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray()));
    }

    [Test]
    public async Task Handle_AutomationUpdateCommand_updatesAutomation()
    {
        var automation = new AutomationModel(id: Guid.NewGuid(), "name", jobs: new[] {new AutomationJobReferenceModel(id: Guid.NewGuid())});
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(automation.Id, automation).Build();

        var command = new AutomationUpdateCommand(automation.Id, "another", new[] {new JobReferenceDto(id: Guid.NewGuid())});
        await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, AutomationModel>(automation.Id);
        value.Should().BeEquivalentTo(
            new AutomationModel(command.Id, command.Name, command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray()));
    }

    [Test]
    public async Task Handle_AutomationUpdateCommand_throwsNotFoundException()
    {
        var automation = new AutomationModel(id: Guid.NewGuid(), "name", jobs: new[] {new AutomationJobReferenceModel(Guid.NewGuid())});
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(automation.Id, automation).Build();

        var command = new AutomationUpdateCommand(id: Guid.NewGuid(), "another", new[] {new JobReferenceDto(id: Guid.NewGuid())});
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }
}
