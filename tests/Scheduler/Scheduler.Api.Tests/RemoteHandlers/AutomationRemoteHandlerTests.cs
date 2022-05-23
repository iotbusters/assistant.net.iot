using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.RemoteHandlers;

[Timeout(2000)]
public class AutomationRemoteHandlerTests
{
    [Test]
    public async Task Handle_AutomationQuery_delegatesQueryAndReturnsAutomationModel()
    {
        var automation = new AutomationModel(
            id: Guid.NewGuid(),
            name: "name",
            jobs: new[] {new AutomationJobReferenceModel(id: Guid.NewGuid())});
        var handler = new TestMessageHandler<AutomationQuery, AutomationModel>(automation);
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceMongoHandler(handler)
            .Build();

        var query = new AutomationQuery(automation.Id);
        var response = await fixture.Handle(query);

        response.Should().BeEquivalentTo(automation);
        handler.Request.Should().BeEquivalentTo(query);
    }

    [Test]
    public async Task Handle_AutomationReferencesQuery_delegatesQueryAndReturnsAutomationReferenceModels()
    {
        var automations = new[] {new AutomationReferenceModel(id: Guid.NewGuid())};
        var handler = new TestMessageHandler<AutomationReferencesQuery, IEnumerable<AutomationReferenceModel>>(automations);
        using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
            .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
            .ReplaceMongoHandler(handler)
            .Build();

        var query = new AutomationReferencesQuery();
        var response = await fixture.Handle(query);

        response.Should().BeEquivalentTo(automations);
    }
}