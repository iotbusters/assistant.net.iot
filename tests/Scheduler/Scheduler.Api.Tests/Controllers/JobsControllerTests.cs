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
    public async Task Get_jobs_id_JobTriggerModel()
    {
        var jobId = Guid.NewGuid();
        var job = new JobTriggerEventModel(
            id: jobId,
            name: "name",
            triggerEventName: "event",
            triggerEventMask: new Dictionary<string, string>());
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
    public async Task Get_jobs_id_JobActionModel()
    {
        var jobId = Guid.NewGuid();
        var job = new JobActionModel(
            id: jobId,
            name: "name",
            action: new TestMessage());
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
    public async Task Post_jobs_JobTriggerModel()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobTriggerCreateCommand, Guid>(_ => jobId);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var command = new JobTriggerCreateCommand(
            name: "name",
            triggerEventName: "Event",
            triggerEventMask: new Dictionary<string, string>());
        var response = await fixture.Post("http://localhost/api/jobs", new JobCreateModel
        {
            Name = command.Name,
            TriggerEventName = command.TriggerEventName,
            TriggerEventMask = command.TriggerEventMask
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
    public async Task Post_jobs_JobActionModel()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobActionCreateCommand, Guid>(_ => jobId);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var command = new JobActionCreateCommand(
            name: "name",
            action: new TestMessage());
        var response = await fixture.Post("http://localhost/api/jobs", new JobCreateModel
        {
            Name = command.Name,
            Action = command.Action
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
    public async Task Put_jobs_id_JobTriggerModel()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobTriggerUpdateCommand, None>(_ => new None());
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var command = new JobTriggerUpdateCommand(
            id: jobId,
            name: "name",
            triggerEventName: "Event",
            triggerEventMask: new Dictionary<string, string>());
        var response = await fixture.Put($"http://localhost/api/jobs/{jobId}", new JobUpdateModel
        {
            Name = command.Name,
            TriggerEventName = command.TriggerEventName,
            TriggerEventMask = command.TriggerEventMask
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
            Content = fixture.NoContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Put_jobs_id_JobActionModel()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobActionUpdateCommand, None>(_ => new None());
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var command = new JobActionUpdateCommand(
            id: jobId,
            name: "name",
            action: new TestMessage());
        var response = await fixture.Put($"http://localhost/api/jobs/{jobId}", new JobUpdateModel
        {
            Name = command.Name,
            Action = command.Action
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
            Content = fixture.NoContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Delete_jobs_id()
    {
        var jobId = Guid.NewGuid();
        var handler = new TestMessageHandler<JobDeleteCommand, None>(_ => new None());
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