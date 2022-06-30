using Assistant.Net.Storage;
using Assistant.Net.Storage.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Integration.Tests;

[SetUpFixture]
public class SetupSqlite
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await MasterConnection.OpenAsync(ShortCancellationToken);
        Provider = new ServiceCollection()
            .AddStorage(b => b.UseSqlite(o => o.Connection(ConnectionString)))
            .BuildServiceProvider();
        var dbContextFactory = Provider.GetRequiredService<IDbContextFactory<StorageDbContext>>();
        var dbContext = await dbContextFactory.CreateDbContextAsync(ShortCancellationToken);
        await dbContext.Database.EnsureCreatedAsync(LongCancellationToken);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Provider?.Dispose();
        MasterConnection.Dispose();
    }

    /// <summary>
    ///     Shared SQLite in-memory database connection string (see <see cref="MasterConnection"/>).
    /// </summary>
    public const string ConnectionString = "Data Source=test;Mode=Memory;Cache=Shared";
    /// <summary>
    ///     Shared SQLite in-memory database connection keeping the data shared between other connections.
    /// </summary>
    public SqliteConnection MasterConnection { get; } = new(ConnectionString);
    private static CancellationToken ShortCancellationToken => new CancellationTokenSource(100).Token;
    private static CancellationToken LongCancellationToken => new CancellationTokenSource(5000).Token;
    private ServiceProvider? Provider { get; set; }
}
