using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Enums;
using Assistant.Net.Scheduler.Api.Models;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Assistant.Net.Scheduler.Api.Tests.Mocks;

namespace Assistant.Net.Scheduler.Api.Tests.Controllers
{
    public class JobsControllerTests
    {
        [Test]
        public async Task Get_jobs_id()
        {
            var jobId = Guid.NewGuid();
            var job = new JobModel(jobId, "name", JobTriggerType.None, null, JobType.Nothing, null);
            var handler = new TestMessageHandler<JobQuery, JobModel>(_ => job);
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Get($"http://localhost/api/jobs/{jobId}");

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.OK,
                RequestMessage = new {RequestUri = new Uri($"http://localhost/api/jobs/{jobId}")},
                Content = fixture.Content(job)
            });
        }

        [Test]
        public async Task Post_jobs()
        {
            var jobId = Guid.NewGuid();
            var handler = new TestMessageHandler<JobCreateCommand, Guid>(_ => jobId);
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Post("http://localhost/api/jobs", new
            {
                Name = "name",
                Trigger = JobTriggerType.EventTrigger,
                TriggerEventMask = new Dictionary<string, string>(),
                Type = JobType.Event,
                Parameters = new Dictionary<string, string>()
            });

            var a = await response.Content.ReadAsStringAsync();
            var b = await fixture.Content(JobType.Event).ReadAsStringAsync();

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.Created,
                RequestMessage = new {RequestUri = new Uri("http://localhost/api/jobs")},
                Headers = new {Location = new Uri($"http://localhost/api/jobs/{jobId}")},
                Content = fixture.CreatedContent()
            });
        }

        [Test]
        public async Task Put_jobs_id()
        {
            var jobId = Guid.NewGuid();
            var handler = new TestMessageHandler<JobUpdateCommand, None>(_ => new None());
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Put($"http://localhost/api/jobs/{jobId}", new
            {
                Name = "name",
                Trigger = JobTriggerType.EventTrigger,
                TriggerEventMask = new Dictionary<string, string>(),
                Type = JobType.Event,
                Parameters = new Dictionary<string, string>()
            });

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NoContent,
                RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
                Content = fixture.NoContent()
            });
        }

        [Test]
        public async Task Delete_jobs_id()
        {
            var jobId = Guid.NewGuid();
            var handler = new TestMessageHandler<JobDeleteCommand, None>(_ => new None());
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Delete($"http://localhost/api/jobs/{jobId}");

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NoContent,
                RequestMessage = new { RequestUri = new Uri($"http://localhost/api/jobs/{jobId}") },
                Content = fixture.NoContent()
            });
        }
    }
}