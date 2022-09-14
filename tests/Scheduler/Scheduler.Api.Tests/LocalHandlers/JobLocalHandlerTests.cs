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

namespace Assistant.Net.Scheduler.Api.Tests.LocalHandlers;

public sealed class JobLocalHandlerTests
{
    [Test]
    public async Task Handle_JobQuery_returnsJobWithEventConfiguration()
    {
        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var command = new JobQuery(job.Id);
        var response = await fixture.Handle(command);

        response.Should().BeEquivalentTo(job);
    }

    [Test]
    public async Task Handle_JobQuery_returnsJobWithDailyTimerConfiguration()
    {
        var configuration = new JobDailyTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var command = new JobQuery(job.Id);
        var response = await fixture.Handle(command);

        response.Should().BeEquivalentTo(job);
    }

    [Test]
    public async Task Handle_JobQuery_returnsJobWithStopwatchTimerConfiguration()
    {
        var configuration = new JobStopwatchTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var command = new JobQuery(job.Id);
        var response = await fixture.Handle(command);

        response.Should().BeEquivalentTo(job);
    }

    [Test]
    public async Task Handle_JobQuery_throwsNotFoundException()
    {
        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var command = new JobQuery(id: Guid.NewGuid());
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }
        
    [Test]
    public async Task Handle_JobCreateCommand_createsJobWithEventConfiguration()
    {
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Build();

        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var command = new JobCreateCommand(name: "name", configuration);
        var id = await fixture.Handle(command);
            
        var value = await fixture.GetOrDefault<Guid, JobModel>(id);
        value.Should().BeEquivalentTo(new JobModel(id, command.Name, configuration));
    }

    [Test]
    public async Task Handle_JobUpdateCommand_updatesJobWithEventConfiguration()
    {
        var configuration = new JobEventConfigurationDto(
            eventName: "Event",
            eventMask: new Dictionary<string, string>());
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var newConfiguration = new JobEventConfigurationDto(
            eventName: nameof(TimerTriggeredEvent),
            eventMask: new Dictionary<string, string> {{"1", "2"}});
        var command = new JobUpdateCommand(job.Id, name: "another", newConfiguration);
        await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, JobModel>(job.Id);
        value.Should().BeEquivalentTo(new JobModel(job.Id, command.Name, newConfiguration));
    }

    [Test]
    public async Task Handle_JobCreateCommand_createsJobWithDailyTimerConfiguration()
    {
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Build();

        var configuration = new JobDailyTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var command = new JobCreateCommand(name: "name", configuration);
        var id = await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, JobModel>(id);
        value.Should().BeEquivalentTo(new JobModel(id, command.Name, configuration));
    }

    [Test]
    public async Task Handle_JobUpdateCommand_updatesJobWithDailyTimerConfiguration()
    {
        var configuration = new JobEventConfigurationDto(
            eventName: "Event",
            eventMask: new Dictionary<string, string>());
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var newConfiguration = new JobDailyTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var command = new JobUpdateCommand(job.Id, name: "another", newConfiguration);
        await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, JobModel>(job.Id);
        value.Should().BeEquivalentTo(new JobModel(job.Id, command.Name, newConfiguration));
    }

    [Test]
    public async Task Handle_JobCreateCommand_createsJobWithStopwatchTimerConfiguration()
    {
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Build();

        var configuration = new JobStopwatchTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var command = new JobCreateCommand(name: "name", configuration);
        var id = await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, JobModel>(id);
        value.Should().BeEquivalentTo(new JobModel(id, command.Name, configuration));
    }

    [Test]
    public async Task Handle_JobUpdateCommand_updatesJobWithStopwatchTimerConfiguration()
    {
        var configuration = new JobEventConfigurationDto(
            eventName: "Event",
            eventMask: new Dictionary<string, string>());
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var newConfiguration = new JobStopwatchTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var command = new JobUpdateCommand(job.Id, name: "another", newConfiguration);
        await fixture.Handle(command);

        var value = await fixture.GetOrDefault<Guid, JobModel>(job.Id);
        value.Should().BeEquivalentTo(new JobModel(job.Id, command.Name, newConfiguration));
    }

    [Test]
    public async Task Handle_JobUpdateCommand_throwsNotFoundException()
    {
        var configuration = new JobEventConfigurationDto(
            eventName: "Event",
            eventMask: new Dictionary<string, string>());
        var job = new JobModel(id: Guid.NewGuid(), name: "name", configuration);
        using var fixture = new SchedulerLocalApiHandlerFixtureBuilder().Store(job.Id, job).Build();

        var newConfiguration = new JobEventConfigurationDto(
            eventName: nameof(TimerTriggeredEvent),
            eventMask: new Dictionary<string, string> {{"1", "2"}});
        var command = new JobUpdateCommand(id: Guid.NewGuid(), name: "another", newConfiguration);
        await fixture.Awaiting(x => x.Handle(command))
            .Should().ThrowAsync<NotFoundException>();
    }
}
