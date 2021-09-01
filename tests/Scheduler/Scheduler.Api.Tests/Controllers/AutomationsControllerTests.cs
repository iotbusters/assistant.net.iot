using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
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
    public class AutomationsControllerTests
    {
        [Test]
        public async Task Get_automations()
        {
            var automations = new[] {new AutomationReferenceModel(Guid.NewGuid())};
            var handler = new TestMessageHandler<AutomationReferencesQuery, IEnumerable<AutomationReferenceModel>>(_ => automations);
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Get("http://localhost/api/automations");

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.OK,
                RequestMessage = new {RequestUri = new Uri("http://localhost/api/automations")},
                Content = fixture.Content(automations)
            });
        }

        [Test]
        public async Task Get_automations_id()
        {
            var automation = new AutomationModel(Guid.NewGuid(), "name", new[] {new AutomationJobReferenceModel(Guid.NewGuid())});
            var handler = new TestMessageHandler<AutomationQuery, AutomationModel>(_ => automation);
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Get($"http://localhost/api/automations/{automation.Id}");

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.OK,
                RequestMessage = new {RequestUri = new Uri($"http://localhost/api/automations/{automation.Id}")},
                Content = fixture.Content(automation)
            });
        }

        [Test]
        public async Task Post_automations()
        {
            var automationId = Guid.NewGuid();
            var handler = new TestMessageHandler<AutomationCreateCommand, Guid>(_ => automationId);
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Post("http://localhost/api/automations", new
            {
                Name = "name",
                Jobs = new[] { new { Id = Guid.NewGuid() } }
            });

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.Created,
                RequestMessage = new {RequestUri = new Uri("http://localhost/api/automations")},
                Headers = new {Location = new Uri($"http://localhost/api/automations/{automationId}")},
                Content = fixture.CreatedContent()
            });
        }

        [Test]
        public async Task Put_automations_id()
        {
            var automationId = Guid.NewGuid();
            var handler = new TestMessageHandler<AutomationUpdateCommand, None>(_ => new None());
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Put($"http://localhost/api/automations/{automationId}", new
            {
                Name = "name",
                Jobs = new[] { new { Id = Guid.NewGuid() } }
            });

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NoContent,
                RequestMessage = new { RequestUri = new Uri($"http://localhost/api/automations/{automationId}") },
                Content = fixture.NoContent()
            });
        }

        [Test]
        public async Task Delete_automations_id()
        {
            var automationId = Guid.NewGuid();
            var handler = new TestMessageHandler<AutomationDeleteCommand, None>(_ => new None());
            using var fixture = new SchedulerApiFixtureBuilder().Add(handler).Build();

            var response = await fixture.Delete($"http://localhost/api/automations/{automationId}");

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NoContent,
                RequestMessage = new { RequestUri = new Uri($"http://localhost/api/automations/{automationId}") },
                Content = fixture.NoContent()
            });
        }
    }
}