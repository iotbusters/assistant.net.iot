using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.EventHandler.Tests.Mocks;
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Integration.Tests;

public sealed class FlowTests
{
    // todo: cover other flows

    [Test]
    public async Task AutomationRun()
    {
        var watch = Stopwatch.StartNew();

        var handler = new TestMessageHandler<TestEmptyEventMessage, Nothing>(Nothing.Instance);
        using var apiFixture = new RemoteHandlerFixtureBuilder<Scheduler.Api.Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .ReplaceHandler(handler)
            .Build();
        using var eventFixture = new RemoteHandlerFixtureBuilder<Scheduler.EventHandler.Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .Build();
        using var triggerFixture = new RemoteHandlerFixtureBuilder<Scheduler.Trigger.Startup>()
            .UseSqlite(SetupSqlite.ConnectionString)
            .Build();

        Console.WriteLine($"[Started] {watch.Elapsed}"); // 3.9s / 2.2s
        watch.Reset();
        watch.Start();

        var delay = TimeSpan.FromSeconds(0.1);
        var jobIds = await Task.WhenAll(
            apiFixture.Client.Request(new JobCreateCommand(name: "t1", new JobStopwatchTimerConfigurationDto(delay))),
            apiFixture.Client.Request(new JobCreateCommand(name: "a1", new JobActionConfigurationDto(new TestEmptyEventMessage()))),
            apiFixture.Client.Request(new JobCreateCommand(name: "t2", new JobStopwatchTimerConfigurationDto(delay))),
            apiFixture.Client.Request(new JobCreateCommand(name: "a1", new JobActionConfigurationDto(new TestEmptyEventMessage())))
        );
        var jobs = jobIds.Select(x => new JobReferenceDto(x)).ToArray();
        var automationId = await apiFixture.Client.Request(new AutomationCreateCommand(name: "a1", jobs));
        var runId = await apiFixture.Client.Request(new RunCreateCommand(automationId));

        Console.WriteLine($"[Arranged] {watch.Elapsed}"); // 1.5s / 1s
        watch.Reset();

        await Task.Delay(TimeSpan.FromSeconds(3));

        watch.Start();

        var run1 = await apiFixture.Client.Request(new RunQuery(runId));
        run1.Status.Should().Be(RunStatus.Succeeded);
        var run2 = await apiFixture.Client.Request(new RunQuery(run1.NextRunId!.Value));
        run2.Status.Should().Be(RunStatus.Succeeded);

        Console.WriteLine($"[Assert-1] {watch.Elapsed}"); // 0.5s / 0.5s
        watch.Reset();

        await Task.Delay(TimeSpan.FromSeconds(3));

        watch.Start();

        var run3 = await apiFixture.Client.Request(new RunQuery(run2.NextRunId!.Value));
        run3.Status.Should().Be(RunStatus.Succeeded);
        var run4 = await apiFixture.Client.Request(new RunQuery(run3.NextRunId!.Value));
        run4.Status.Should().Be(RunStatus.Succeeded);

        Console.WriteLine($"[Assert-2] {watch.Elapsed}"); // 1s / 0.8s
        watch.Reset();

        handler.Requests.Should().HaveCountGreaterOrEqualTo(2);
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
