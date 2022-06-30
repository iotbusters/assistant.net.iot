using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Controllers;

public class JobsControllerTests
{
    [Test]
    public async Task Get_jobs_id_JobEventTriggerModel()
    {
        var jobId = Guid.NewGuid();
        var configuration = new JobEventConfigurationDto(
            eventName: "Event",
            eventMask: new Dictionary<string, string>());
        var job = new JobModel(jobId, name: "name", configuration);
        var handler = new TestMessageHandler<JobQuery, JobModel>(_ => job);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get($"http://localhost/api/jobs/{jobId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new {RequestUri = new Uri($"http://localhost/api/jobs/{jobId}")},
            Content = fixture.Content(job)
        });
        handler.Request.Should().BeEquivalentTo(new JobQuery(jobId));
    }
    [Test]
    public async Task Get_jobs_id_JobDailyTimerTriggerModel()
    {
        var jobId = Guid.NewGuid();
        var configuration = new JobDailyTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var job = new JobModel(jobId, name: "name", configuration);
        var handler = new TestMessageHandler<JobQuery, JobModel>(_ => job);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get($"http://localhost/api/jobs/{jobId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
            Content = fixture.Content(job)
        });
        handler.Request.Should().BeEquivalentTo(new JobQuery(jobId));
    }

    [Test]
    public async Task Get_jobs_id_JobStopwatchTimerTriggerModel()
    {
        var jobId = Guid.NewGuid();
        var configuration = new JobStopwatchTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var job = new JobModel(jobId, name: "name", configuration);
        var handler = new TestMessageHandler<JobQuery, JobModel>(_ => job);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get($"http://localhost/api/jobs/{jobId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
            Content = fixture.Content(job)
        });
        handler.Request.Should().BeEquivalentTo(new JobQuery(jobId));
    }

    [Test]
    public async Task Get_jobs_id_JobActionModel()
    {
        var jobId = Guid.NewGuid();
        var configuration = new JobActionConfigurationDto(new TestEmptyMessage());
        var job = new JobModel(jobId, name: "name", configuration);
        var handler = new TestMessageHandler<JobQuery, JobModel>(_ => job);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get($"http://localhost/api/jobs/{jobId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new {RequestUri = new Uri($"http://localhost/api/jobs/{jobId}")},
            Content = fixture.Content(job)
        });
        handler.Request.Should().BeEquivalentTo(new JobQuery(jobId));
    }

    [Test]
    public async Task Post_jobs_JobEventCreateModel()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobCreateCommand, Guid>(_ => jobId);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var configuration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var command = new JobCreateCommand(name: "name", configuration);
        var response = await fixture.Post("http://localhost/api/jobs", new JobEventCreateModel
        {
            Name = command.Name,
            EventName = configuration.EventName,
            EventMask = configuration.EventMask
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.Created,
            RequestMessage = new {RequestUri = new Uri("http://localhost/api/jobs")},
            Headers = new {Location = new Uri($"http://localhost/api/jobs/{jobId}")},
            Content = fixture.CreatedContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Post_jobs_JobTimerCreateModel()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobCreateCommand, Guid>(_ => jobId);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var configuration = new JobDailyTimerConfigurationDto(time: TimeSpan.FromHours(1));
        var command = new JobCreateCommand(name: "name", configuration);
        var response = await fixture.Post("http://localhost/api/jobs", new JobDailyTimerCreateModel
        {
            Name = command.Name,
            Time = configuration.Time,
            Days = configuration.Days
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.Created,
            RequestMessage = new {RequestUri = new Uri("http://localhost/api/jobs")},
            Headers = new {Location = new Uri($"http://localhost/api/jobs/{jobId}")},
            Content = fixture.CreatedContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Post_jobs_JobActionCreateModel()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobCreateCommand, Guid>(_ => jobId);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var configuration = new JobActionConfigurationDto(new TestEmptyMessage());
        var command = new JobCreateCommand(name: "name", configuration);
        var response = await fixture.Post("http://localhost/api/jobs", new JobActionCreateModel
        {
            Name = command.Name,
            Action = configuration.Action
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.Created,
            RequestMessage = new {RequestUri = new Uri("http://localhost/api/jobs")},
            Headers = new {Location = new Uri($"http://localhost/api/jobs/{jobId}")},
            Content = fixture.CreatedContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Put_jobs_id_JobUpdateModel()
    {
        var jobId = Guid.NewGuid();
        var configuration = new JobActionConfigurationDto(new TestEmptyMessage());
        var job = new JobModel(jobId, name: "name", configuration);
        var handler1 = new TestMessageHandler<JobQuery, JobModel>(_ => job);
        var handler2 = new TestMessageHandler<JobUpdateCommand, Nothing>(_ => Nothing.Instance);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler1).ReplaceApiHandler(handler2).Build();

        var newConfiguration = new JobEventConfigurationDto(eventName: "Event", eventMask: new Dictionary<string, string>());
        var command = new JobUpdateCommand(jobId, name: "name", newConfiguration);
        var response = await fixture.Put($"http://localhost/api/jobs/{jobId}", new JobUpdateModel {Name = command.Name});

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
            Content = fixture.NoContent()
        });
        handler1.Request.Should().BeEquivalentTo(new JobQuery(jobId));
        handler2.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Delete_jobs_id()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobDeleteCommand, Nothing>(_ => Nothing.Instance);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Delete($"http://localhost/api/jobs/{jobId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
            Content = fixture.NoContent()
        });
        handler.Request.Should().BeEquivalentTo(new JobDeleteCommand(jobId));
    }
}
