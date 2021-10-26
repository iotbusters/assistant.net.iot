using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Messaging
{
    [Timeout(2000)]
    public class JobRemoteHandlerTests
    {
        [Test]
        public async Task Handle_JobQuery_delegatesQueryAndReturnsJobModel()
        {
            var job = new JobModel(
                id: Guid.NewGuid(),
                name: "name",
                trigger: JobTriggerType.None,
                triggerEventMask: null,
                type: JobType.Nothing,
                parameters: null);
            var handler = new TestMessageHandler<JobQuery, JobModel>(job);
            using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var query = new JobQuery(job.Id);
            var response = await fixture.Handle(query);

            response.Should().BeEquivalentTo(job);
            handler.Request.Should().BeEquivalentTo(query);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Provider = new ServiceCollection()
                .ConfigureMongoOptions(ConnectionString)
                .AddMongoClientFactory()
                .BuildServiceProvider();

            string pingContent;
            var client = Provider.GetRequiredService<IMongoClientFactory>().Create();
            try
            {
                var ping = await client.GetDatabase("db").RunCommandAsync(
                    (Command<BsonDocument>)"{ping:1}",
                    ReadPreference.Nearest,
                    new CancellationTokenSource(200).Token);
                pingContent = ping.ToString();
            }
            catch
            {
                pingContent = string.Empty;
            }
            if (!pingContent.Contains("ok"))
                Assert.Ignore($"The tests require mongodb instance at {ConnectionString}.");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => Provider.Dispose();

        [SetUp, TearDown]
        public async Task Cleanup()
        {
            var client = Provider.GetRequiredService<IMongoClientFactory>().Create();
            await client.DropDatabaseAsync(Database);
        }

        private const string ConnectionString = "mongodb://127.0.0.1:27017";
        private const string Database = "test";
        public ServiceProvider Provider { get; set; } = default!;
    }
}
