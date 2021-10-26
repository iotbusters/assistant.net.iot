using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Messaging
{
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
                .UseMongo(ConnectionString, Database)
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
                .UseMongo(ConnectionString, Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var query = new AutomationReferencesQuery();
            var response = await fixture.Handle(query);

            response.Should().BeEquivalentTo(automations);
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
