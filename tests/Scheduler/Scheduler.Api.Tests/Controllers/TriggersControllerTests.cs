using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Controllers;

public sealed class TriggersControllerTests
{
    [Test]
    public async Task Get_triggers()
    {
        var runId = Guid.NewGuid();
        var triggers = new[] {new TriggerReferenceModel(runId)};
        var handler = new TestMessageHandler<TriggerReferencesQuery, IEnumerable<TriggerReferenceModel>>(triggers);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get("http://localhost/api/triggers");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new {RequestUri = new Uri("http://localhost/api/triggers")},
            Content = fixture.Content(triggers)
        });
        handler.Request.Should().BeOfType<TriggerReferencesQuery>();
    }

    [Test]
    public async Task Get_triggers_id()
    {
        var runId = Guid.NewGuid();
        var trigger = new TriggerModel(
            runId,
            true,
            "event",
            new Dictionary<string, string>());
        var handler = new TestMessageHandler<TriggerQuery, TriggerModel>(_ => trigger);
        using var fixture = new SchedulerApiFixtureBuilder().ReplaceApiHandler(handler).Build();

        var response = await fixture.Get($"http://localhost/api/triggers/{runId}");

        response.Should().BeEquivalentTo(new
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = new {RequestUri = new Uri($"http://localhost/api/triggers/{runId}")},
            Content = fixture.Content(trigger)
        });
        handler.Request.Should().BeEquivalentTo(new TriggerQuery(runId));
    }
}
