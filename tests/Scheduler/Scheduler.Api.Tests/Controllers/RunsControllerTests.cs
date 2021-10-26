using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Controllers
{
    public class RunsControllerTests
    {
        [Test]
        public async Task Get_Runs_id()
        {
            var runId = Guid.NewGuid();
            var snapshot = new JobModel(
                id: Guid.NewGuid(),
                name: "name",
                trigger: JobTriggerType.None,
                triggerEventMask: null,
                type: JobType.Nothing,
                parameters: null);
            var run = new RunModel(runId, nextRunId: Guid.NewGuid(), automationId: Guid.NewGuid(), snapshot);
            var handler = new TestMessageHandler<RunQuery, RunModel>(_ => run);
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

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
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

            var command = new RunCreateCommand(automationId: Guid.NewGuid());
            var response = await fixture.Post("http://localhost/api/runs", command);

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
            var handler = new TestMessageHandler<RunUpdateCommand, None>(_ => new None());
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

            var command = new RunUpdateCommand(runId, new RunStatusDto(RunStatus.Started, "message"));
            var response = await fixture.Put($"http://localhost/api/runs/{runId}", command);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NoContent,
                RequestMessage = new { RequestUri = new Uri($"http://localhost/api/runs/{runId}") },
                Content = fixture.NoContent()
            });
            handler.Request.Should().BeEquivalentTo(command);
        }

        [Test]
        public async Task Delete_Runs_id()
        {
            var runId = Guid.NewGuid();
            var handler = new TestMessageHandler<RunDeleteCommand, None>(_ => new None());
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

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
}
