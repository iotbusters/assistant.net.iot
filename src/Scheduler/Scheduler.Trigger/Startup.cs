using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Assistant.Net.Scheduler.Trigger.Internal;
using Assistant.Net.Scheduler.Trigger.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Scheduler.Trigger
{
    /// <summary/>
    public class Startup
    {
        /// <summary/>
        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        /// <summary/>
        public IConfiguration Configuration { get; }

        /// <summary/>
        public void ConfigureServices(IServiceCollection services) => services
            .Configure<TriggerPollingOptions>(Configuration.GetSection("TriggerPolling"))
            .AddHostedService<TriggerPollingService>()
            .AddMongoMessageHandling(b => b
                .UseMongo(ConfigureMongo))
            .AddMessagingClient(b => b
                .UseMongo(ConfigureMongo)
                .AddMongo<TriggerQuery>()
                .AddMongo<TriggerReferencesQuery>())
            .AddSingleton<ReloadableOptionsSource>()
            .AddOptions<MessagingClientOptions>(MongoOptionsNames.DefaultName)
            .Bind<ReloadableOptionsSource>()
            .Services
            .AddSingleton<IMessageHandlerFactory, DefaultMessageHandlerFactory>();

        private void ConfigureMongo(MongoOptions options) => options
            .Connection(Configuration.GetConnectionString(Contracts.ConfigurationNames.Messaging))
            .Database(Contracts.MongoNames.DatabaseName);
    }
}
