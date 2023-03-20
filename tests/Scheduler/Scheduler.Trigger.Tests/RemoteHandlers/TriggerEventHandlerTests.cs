using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Models;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Test.Common.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Tests.RemoteHandlers;

public sealed class TriggerEventHandlerTests
{
    [Test]
    public async Task Request_TriggerCreatedEvent_configuresEventTriggerOptions_jobEventConfiguration()
    {
        var runId = Guid.NewGuid();
        var handler1 = new TestMessageHandler<RunQuery, RunModel>(query => new RunModel(
            query.Id,
            nextRunId: null,
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobModel(
                id: Guid.NewGuid(),
                name: "test",
                configuration: new JobEventConfigurationDto(
                    eventName: nameof(TestEvent),
                    eventMask: new Dictionary<string, string>()))));
        var handler2 = new TestMessageHandler<TriggerQuery, TriggerModel>(query => new TriggerModel(
            query.RunId,
            isActive: true,
            triggerEventName: nameof(TestEvent),
            triggerEventMask: new Dictionary<string, string>()));
        using var fixture = new RemoteHandlerFixtureBuilder<Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceLocalHandler(handler1)
            .ReplaceLocalHandler(handler2)
            .Build();
        await fixture.Client.Request(new TriggerCreatedEvent(runId));
        await Task.Delay(TimeSpan.FromSeconds(1));

        handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(runId)});
        handler2.Requests.Should().BeEquivalentTo(new[] {new TriggerQuery(runId)});
        fixture.HostService<IOptionsMonitor<EventTriggerOptions>>().Get(GenericOptionsNames.DefaultName)
            .Should().BeEquivalentTo(new EventTriggerOptions
            {
                EventTriggers = new Dictionary<Type, ISet<Guid>> {[typeof(TestEvent)] = new HashSet<Guid> {runId}}
            });
    }

    [Test]
    public async Task Request_TriggerCreatedEvent_throwsNotFoundException_noRun()
    {
        var runId = Guid.NewGuid();
        var handler1 = new TestMessageHandler<RunQuery, RunModel>(_ =>
            new NotFoundException().Throw<RunModel>());
        var handler2 = new TestMessageHandler<TriggerQuery, TriggerModel>(query => new TriggerModel(
            query.RunId,
            isActive: true,
            triggerEventName: nameof(TestEvent),
            triggerEventMask: new Dictionary<string, string>()));
        using var fixture = new RemoteHandlerFixtureBuilder<Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceLocalHandler(handler1)
            .ReplaceLocalHandler(handler2)
            .Build();
        await fixture.Invoking(x => x.Client.Request(new TriggerCreatedEvent(runId)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Request_TriggerCreatedEvent_throwsNotFoundException_noTrigger()
    {
        var runId = Guid.NewGuid();
        var handler1 = new TestMessageHandler<RunQuery, RunModel>(query => new RunModel(
            query.Id,
            nextRunId: null,
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobModel(
                id: Guid.NewGuid(),
                name: "test",
                configuration: new JobEventConfigurationDto(
                    eventName: nameof(TestEvent),
                    eventMask: new Dictionary<string, string>()))));
        var handler2 = new TestMessageHandler<TriggerQuery, TriggerModel>(_ =>
            new NotFoundException().Throw<TriggerModel>());
        using var fixture = new RemoteHandlerFixtureBuilder<Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceLocalHandler(handler1)
            .ReplaceLocalHandler(handler2)
            .Build();
        await fixture.Invoking(x => x.Client.Request(new TriggerCreatedEvent(runId)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Request_TriggerCreatedEvent_configuresEventTriggerOptions_jobStopwatchTimerConfiguration()
    {
        var runId = Guid.NewGuid();
        var handler1 = new TestMessageHandler<RunQuery, RunModel>(query => new RunModel(
            query.Id,
            nextRunId: null,
            automationId: Guid.NewGuid(),
            jobSnapshot: new JobModel(
                id: Guid.NewGuid(),
                name: "test",
                configuration: new JobStopwatchTimerConfigurationDto(time: TimeSpan.FromMilliseconds(10)))));
        var handler2 = new TestMessageHandler<TriggerQuery, TriggerModel>(query => new TriggerModel(
            query.RunId,
            isActive: true,
            triggerEventName: nameof(TimerTriggeredEvent),
            triggerEventMask: new Dictionary<string, string>()));
        using var fixture = new RemoteHandlerFixtureBuilder<Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceLocalHandler(handler1)
            .ReplaceLocalHandler(handler2)
            .Build();

        await fixture.Client.Request(new TriggerCreatedEvent(runId));
        await Task.Delay(TimeSpan.FromSeconds(1));

        handler1.Requests.Should().BeEquivalentTo(new[] {new RunQuery(runId)});
        handler2.Requests.Should().BeEquivalentTo(new[] {new TriggerQuery(runId)});
        fixture.HostService<IOptionsMonitor<EventTriggerOptions>>().Get(GenericOptionsNames.DefaultName)
            .Should().BeEquivalentTo(new EventTriggerOptions
            {
                EventTriggers = new Dictionary<Type, ISet<Guid>> {[typeof(TimerTriggeredEvent)] = new HashSet<Guid> {runId}}
            });
    }

    [TearDown]
    public async Task TearDown()
    {
        await using var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .AddSqlite<Guid, TriggerTimerModel>())
            .BuildServiceProvider();
        var dbcontext = provider.GetRequiredService<StorageDbContext>();
        var a = await dbcontext.StorageKeys.ToArrayAsync();
        var storage = provider.GetRequiredService<IAdminStorage<Guid, TriggerTimerModel>>();
        await foreach (var key in storage.GetKeys())
            await storage.TryRemove(key);
    }
}
