using Assistant.Net.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Tests.RemoteHandlers;

[SetUpFixture]
public class SetupMongo
{
    [OneTimeSetUp]
    public static async Task SetUp()
    {
        Provider = new ServiceCollection()
            .ConfigureMongoOptions("", o => o.Connection(ConnectionString))
            .AddMongoClientFactory()
            .BuildServiceProvider();

        string pingContent;
        var client = Provider.GetRequiredService<IMongoClientFactory>().CreateClient("");
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

        await client.DropDatabaseAsync(Database);
    }

    [OneTimeTearDown]
    public static void TearDown() => Provider.Dispose();

    public const string ConnectionString = "mongodb://127.0.0.1:27017";
    public const string Database = "test";

    private static ServiceProvider Provider { get; set; } = default!;
}