using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
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
using System.Net;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Controllers;

public sealed class RunsControllerTests
{
    [Test]
    public async Task Get_Runs_id()
    {
        var runId = Guid.NewGuid();
        var snapshot = new JobModel(
            id: Guid.NewGuid(),
            name: "name",
            new JobEventConfigurationDto(
                eventName: "Event",
                eventMask: new Dictionary<string, string>()));
        var run = new RunModel(runId, nextRunId: Guid.NewGuid(), automationId: Guid.NewGuid(), snapshot);
        var handler = new TestMessageHandler<RunQuery, RunModel>(_ => run);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get($"http://localhost/api/runs/{runId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new {RequestUri = new Uri($"http://localhost/api/runs/{runId}")},
            Content = fixture.Content(run)
        });
        handler.Request.Should().BeEquivalentTo(new RunQuery(runId));
    }

    [Test]
    public async Task Post_Runs()
    {
        var runId = Guid.NewGuid();
        var handler = new TestMessageHandler<RunCreateCommand, Guid>(_ => runId);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var command = new RunCreateCommand(automationId: Guid.NewGuid());
        var response = await fixture.Post("http://localhost/api/runs", new RunCreateModel
        {
            AutomationId = command.AutomationId
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.Created,
            RequestMessage = new {RequestUri = new Uri("http://localhost/api/runs")},
            Headers = new {Location = new Uri($"http://localhost/api/runs/{runId}")},
            Content = fixture.CreatedContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Put_Runs_id()
    {
        var runId = Guid.NewGuid();
        var handler = new TestMessageHandler<RunStartCommand, Nothing>(_ => Nothing.Instance);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Put($"http://localhost/api/runs/{runId}", new RunUpdateModel
        {
            Status = RunStatus.Started
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/runs/{runId}") },
            Content = fixture.NoContent()
        });
        handler.Request.Should().BeEquivalentTo(new RunStartCommand(runId));
    }

    [Test]
    public async Task Delete_Runs_id()
    {
        var runId = Guid.NewGuid();
        var handler = new TestMessageHandler<RunDeleteCommand, Nothing>(_ => Nothing.Instance);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Delete($"http://localhost/api/runs/{runId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/runs/{runId}") },
            Content = fixture.NoContent()
        });
        handler.Request.Should().BeEquivalentTo(new RunDeleteCommand(runId));
    }
}
