using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.EventHandler.Tests.Mocks;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Models;
using Assistant.Net.Test.Common;
using Assistant.Net.Test.Common.Fixtures;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Integration.Tests;

public sealed class Tests
{
    [Test]
    public async Task Test()
    {
        var handler = new TestMessageHandler<TestEmptyEventMessage, Nothing>(Nothing.Instance);
        using var apiFixture = new RemoteHandlerFixtureBuilder<Scheduler.Api.Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceHandler(handler)
            .Build(2);
        using var eventFixture = new RemoteHandlerFixtureBuilder<Scheduler.EventHandler.Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .Build(2);
        using var triggerFixture = new RemoteHandlerFixtureBuilder<Scheduler.Trigger.Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ConfigureServer(s => s
                .Configure<TimerTriggerOptions>(o => o.InactivityDelayTime = TimeSpan.FromSeconds(0.5))
                .Configure<TriggerPollingOptions>(o => o.InactivityDelayTime = TimeSpan.FromSeconds(0.5)))
            .Build();

        var delay = TimeSpan.FromSeconds(1);
        var jobIds = await Task.WhenAll(
            apiFixture.Client.Request(new JobCreateCommand(name: "t1", new JobStopwatchTimerConfigurationDto(delay))),
            apiFixture.Client.Request(new JobCreateCommand(name: "a1", new JobActionConfigurationDto(new TestEmptyEventMessage()))),
            apiFixture.Client.Request(new JobCreateCommand(name: "t2", new JobStopwatchTimerConfigurationDto(delay))),
            apiFixture.Client.Request(new JobCreateCommand(name: "a1", new JobActionConfigurationDto(new TestEmptyEventMessage())))
        );
        var jobs = jobIds.Select(x => new JobReferenceDto(x)).ToArray();
        var automationId = await apiFixture.Client.Request(new AutomationCreateCommand(name: "a1", jobs));
        var runId = await apiFixture.Client.Request(new RunCreateCommand(automationId));

        await Task.Delay(TimeSpan.FromSeconds(5));

        var trigger = await apiFixture.Client.Request(new TriggerQuery(runId));
        trigger.IsActive.Should().BeFalse("Trigger must be inactive.");

        var run1 = await apiFixture.Client.Request(new RunQuery(runId));
        run1.Status.Should().Be(RunStatus.Succeeded);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var run2 = await apiFixture.Client.Request(new RunQuery(run1.NextRunId!.Value));
        run2.Status.Should().Be(RunStatus.Succeeded);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var run3 = await apiFixture.Client.Request(new RunQuery(run2.NextRunId!.Value));
        run3.Status.Should().Be(RunStatus.Succeeded);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var run4 = await apiFixture.Client.Request(new RunQuery(run3.NextRunId!.Value));
        run4.Status.Should().Be(RunStatus.Succeeded);

        handler.Requests.Should().NotBeEmpty();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await MasterConnection.OpenAsync(CancellationToken);
        Provider = new ServiceCollection()
            .AddStorage(b => b.UseSqlite(ConnectionString))
            .BuildServiceProvider();
        var dbContext = await Provider.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync(CancellationToken);
        await dbContext.Database.EnsureCreatedAsync(CancellationToken);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Provider?.Dispose();
        MasterConnection.Dispose();
    }

    [TearDown]
    public async Task TearDown()
    {
        var dbContext = await Provider!.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync();
        dbContext.HistoricalKeys.RemoveRange(dbContext.HistoricalKeys);
        dbContext.StorageKeys.RemoveRange(dbContext.StorageKeys);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Shared SQLite in-memory database connection string (see <see cref="MasterConnection"/>).
    /// </summary>
    private const string ConnectionString = "Data Source=test;Mode=Memory;Cache=Shared";
    /// <summary>
    ///     Shared SQLite in-memory database connection keeping the data shared between other connections.
    /// </summary>
    private SqliteConnection MasterConnection { get; } = new(ConnectionString);
    private static CancellationToken CancellationToken => new CancellationTokenSource(5000).Token;
    private ServiceProvider? Provider { get; set; }
}
