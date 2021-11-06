using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.RemoteHandlers
{
    [Timeout(2000)]
    public class JobRemoteHandlerTests
    {
        [Test]
        public async Task Handle_JobQuery_delegatesQueryAndReturnsJobModel()
        {
            var job = new JobTriggerModel(
                id: Guid.NewGuid(),
                name: "name",
                triggerEventName: "Event",
                triggerEventMask: new Dictionary<string, string>());
            var handler = new TestMessageHandler<JobQuery, JobModel>(job);
            using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
                .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var query = new JobQuery(job.Id);
            var response = await fixture.Handle(query);

            response.Should().BeEquivalentTo(job);
            handler.Request.Should().BeEquivalentTo(query);
        }
    }
}
