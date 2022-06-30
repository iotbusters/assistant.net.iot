using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.EventHandler.Tests.Fixtures;
using Assistant.Net.Scheduler.Trigger.Tests.Fixtures;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Integration.Tests;

public class Tests
{
    [Test]
    public async Task Test()
    {
        using var apiFixture = new SchedulerRemoteApiHandlerFixtureBuilder().UseSqlite(SetupSqlite.ConnectionString).Build();
        using var eventFixture = new SchedulerRemoteEventHandlerFixtureBuilder().UseSqlite(SetupSqlite.ConnectionString).Build();
        using var triggerFixture = new SchedulerRemoteTriggerHandlerFixtureBuilder().UseSqlite(SetupSqlite.ConnectionString).Build();

        await Task.Delay(TimeSpan.FromSeconds(1));

        var everyHour = TimeSpan.FromHours(1);
        var jobIds = await Task.WhenAll(
            apiFixture.Handle(new JobCreateCommand(name: "t1", new JobStopwatchTimerConfigurationDto(everyHour))),
            apiFixture.Handle(new JobCreateCommand(name: "a1", new JobActionConfigurationDto(new TestEmptyMessage()))),
            apiFixture.Handle(new JobCreateCommand(name: "t2", new JobStopwatchTimerConfigurationDto(everyHour))),
            apiFixture.Handle(new JobCreateCommand(name: "a1", new JobActionConfigurationDto(new TestEmptyMessage())))
        );
        var jobs = jobIds.Select(x => new JobReferenceDto(x)).ToArray();
        var automationId = await apiFixture.Handle(new AutomationCreateCommand(name: "a1", jobs));
        var runId = await apiFixture.Handle(new RunCreateCommand(automationId));

        var runModel = await apiFixture.Handle(new RunQuery(runId));
        var triggerModel = await apiFixture.Handle(new TriggerQuery(runId));


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
