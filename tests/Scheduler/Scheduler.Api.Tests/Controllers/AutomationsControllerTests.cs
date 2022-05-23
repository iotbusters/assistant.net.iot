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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Controllers;

public class AutomationsControllerTests
{
    [Test]
    public async Task Get_automations()
    {
        var automations = new[] {new AutomationReferenceModel(id: Guid.NewGuid())};
        var handler = new TestMessageHandler<AutomationReferencesQuery, IEnumerable<AutomationReferenceModel>>(_ => automations);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

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
        var automation = new AutomationModel(id: Guid.NewGuid(), "name", new[] {new AutomationJobReferenceModel(id: Guid.NewGuid())});
        var handler = new TestMessageHandler<AutomationQuery, AutomationModel>(_ => automation);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get($"http://localhost/api/automations/{automation.Id}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new {RequestUri = new Uri($"http://localhost/api/automations/{automation.Id}")},
            Content = fixture.Content(automation)
        });
        handler.Request.Should().BeEquivalentTo(new AutomationQuery(automation.Id));
    }

    [Test]
    public async Task Post_automations()
    {
        var automationId = Guid.NewGuid();
        var handler = new TestMessageHandler<AutomationCreateCommand, Guid>(_ => automationId);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var command = new AutomationCreateCommand("name", new[] {new JobReferenceDto(id: Guid.NewGuid())});
        var response = await fixture.Post("http://localhost/api/automations", new AutomationCreateModel
        {
            Name = command.Name,
            Jobs = command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray()
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.Created,
            RequestMessage = new {RequestUri = new Uri("http://localhost/api/automations")},
            Headers = new {Location = new Uri($"http://localhost/api/automations/{automationId}")},
            Content = fixture.CreatedContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Put_automations_id()
    {
        var automationId = Guid.NewGuid();
        var handler = new TestMessageHandler<AutomationUpdateCommand, None>(_ => new None());
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var command = new AutomationUpdateCommand(automationId, "name", new[] {new JobReferenceDto(id: Guid.NewGuid())});
        var response = await fixture.Put($"http://localhost/api/automations/{automationId}", new AutomationUpdateModel
        {
            Name = command.Name,
            Jobs = command.Jobs.Select(x => new AutomationJobReferenceModel(x.Id)).ToArray()
        });

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/automations/{automationId}") },
            Content = fixture.NoContent()
        });
        handler.Request.Should().BeEquivalentTo(command);
    }

    [Test]
    public async Task Delete_automations_id()
    {
        var automationId = Guid.NewGuid();
        var handler = new TestMessageHandler<AutomationDeleteCommand, None>(_ => new None());
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Delete($"http://localhost/api/automations/{automationId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.NoContent,
            RequestMessage = new { RequestUri = new Uri($"http://localhost/api/automations/{automationId}") },
            Content = fixture.NoContent()
        });
        handler.Request.Should().BeEquivalentTo(new AutomationDeleteCommand(automationId));
    }
}