﻿using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.RemoteHandlers;

[Timeout(2000)]
public sealed class JobRemoteHandlerTests
{
    [Test]
    public async Task Handle_JobQuery_delegatesQueryAndReturnsJobModel()
    {
        var job = new JobModel(
            id: Guid.NewGuid(),
            name: "name",
            new JobEventConfigurationDto(
                eventName: "Event",
                eventMask: new Dictionary<string, string>()));
        var handler = new TestMessageHandler<JobQuery, JobModel>(job);
        using var fixture = new SchedulerRemoteApiHandlerFixtureBuilder()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceHandler(handler)
            .Build();

        var query = new JobQuery(job.Id);
        var response = await fixture.Handle(query);

        response.Should().BeEquivalentTo(job);
        handler.Request.Should().BeEquivalentTo(query);
    }
}
