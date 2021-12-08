using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Configuration;
using Assistant.Net.Scheduler.Trigger.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Scheduler.Trigger
{
    /// <summary/>
    public class Startup
    {
        /// <summary>
        ///     Messaging MongoDB database name.
        /// </summary>
        public const string MessagingDatabaseName = "Scheduler";

        /// <summary/>
        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        /// <summary/>
        public IConfiguration Configuration { get; }

        /// <summary/>
        public void ConfigureServices(IServiceCollection services) => services
            .AddHostedService<TriggerPollingService>()
            .AddMongoMessageHandling(b => b.UseMongo(ConfigureMongo))
            .AddMessagingClient(b => b
                .UseMongo(ConfigureMongo)
                .AddMongo<TriggerReferencesQuery>()
                .AddMongo<TriggerQuery>())
            .AddOptions<MessagingClientOptions>(MongoOptionsNames.DefaultName)
            .Bind(typeof(ReloadableOptionsSource));

        private void ConfigureMongo(MongoOptions options) => options
            .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging)).Database(MessagingDatabaseName);
    }
}
