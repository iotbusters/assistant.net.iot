using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
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
    public class RunRemoteHandlerTests
    {
        [Test]
        public async Task Handle_RunQuery_delegatesQueryAndReturnsRunModel()
        {
            var runId = Guid.NewGuid();
            var run = new RunModel(
                id: runId,
                nextRunId: Guid.NewGuid(),
                automationId: Guid.NewGuid(),
                jobSnapshot: new JobModel(
                    id: Guid.NewGuid(),
                    name: "name",
                    trigger: JobTriggerType.None,
                    triggerEventMask: null,
                    type: JobType.Nothing,
                    parameters: null));
            var handler = new TestMessageHandler<RunQuery, RunModel>(run);
            using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var query = new RunQuery(runId);
            var response = await fixture.Handle(query);

            response.Should().BeEquivalentTo(run);
            handler.Request.Should().BeEquivalentTo(query);
        }

        [Test]
        public async Task Handle_RunCreateCommand_delegatesCommandAndReturnsId()
        {
            var runId = Guid.NewGuid();
            var handler = new TestMessageHandler<RunCreateCommand, Guid>(response: runId);
            using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var command = new RunCreateCommand(automationId: Guid.NewGuid());
            var id = await fixture.Handle(command);

            id.Should().Be(runId);
            handler.Request.Should().BeEquivalentTo(command);
        }

        [Test]
        public async Task Handle_RunUpdateCommand_delegatesCommand()
        {
            var handler = new TestMessageHandler<RunUpdateCommand, None>(new None());
            using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var command = new RunUpdateCommand(id: Guid.NewGuid(), new RunStatusDto(RunStatus.Started, "message"));
            await fixture.Handle(command);

            handler.Request.Should().BeEquivalentTo(command);
        }

        [Test]
        public async Task Handle_RunDeleteCommand_delegatesCommand()
        {
            var handler = new TestMessageHandler<RunDeleteCommand, None>(new None());
            using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var command = new RunDeleteCommand(id: Guid.NewGuid());
            await fixture.Handle(command);

            handler.Request.Should().BeEquivalentTo(command);
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
