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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Controllers
{
    public class JobsControllerTests
    {
        [Test]
        public async Task Get_jobs_id()
        {
            var jobId = Guid.NewGuid();
            var job = new JobModel(
                id: jobId,
                name: "name",
                trigger: JobTriggerType.None,
                triggerEventMask: null,
                type: JobType.Nothing,
                parameters: null);
            var handler = new TestMessageHandler<JobQuery, JobModel>(_ => job);
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

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
        public async Task Post_jobs()
        {
            var jobId = Guid.NewGuid();
            var handler = new TestMessageHandler<JobCreateCommand, Guid>(_ => jobId);
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

            var command = new JobCreateCommand(
                name: "name",
                trigger: JobTriggerType.EventTrigger,
                triggerEventMask: new Dictionary<string, string>(),
                type: JobType.Event,
                parameters: new Dictionary<string, string>());
            var response = await fixture.Post("http://localhost/api/jobs", command);

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
        public async Task Put_jobs_id()
        {
            var jobId = Guid.NewGuid();
            var handler = new TestMessageHandler<JobUpdateCommand, None>(_ => new None());
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

            var command = new JobUpdateCommand(
                id: jobId,
                name: "name",
                trigger: JobTriggerType.EventTrigger,
                triggerEventMask: new Dictionary<string, string>(),
                type: JobType.Event,
                parameters: new Dictionary<string, string>());
            var response = await fixture.Put($"http://localhost/api/jobs/{jobId}", new
            {
                command.Name,
                command.Trigger,
                command.TriggerEventMask,
                command.Type,
                command.Parameters
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
            using var fixture = new SchedulerApiFixtureBuilder().ReplaceMongoHandler(handler).Build();

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
}
