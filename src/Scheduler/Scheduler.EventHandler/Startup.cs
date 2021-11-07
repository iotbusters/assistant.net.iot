using Assistant.Net.Messaging;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.EventHandler.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Scheduler.EventHandler
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
            .AddMongoMessageHandling(b => b
                .Use(Configuration.GetConnectionString("RemoteMessageHandling"))
                .AddHandler<TimerTriggeredEventHandler>()
                .AddHandler<RunSucceededEventHandler>()
                .AddHandler<RunFailedEventHandler>())
            .ConfigureMongoHandlingClientOptions(o => o.DatabaseName = "Scheduler")
            .ConfigureMongoHandlingServerOptions(o => o.DatabaseName = "Scheduler")
            .ConfigureMessagingClient(b => b
                .AddMongo<AutomationReferencesQuery>()
                .AddMongo<AutomationQuery>()
                .AddMongo<JobQuery>()
                .AddMongo<RunQuery>()
                .AddMongo<RunCreateCommand>()
                .AddMongo<RunUpdateCommand>()
                .AddMongo<RunDeleteCommand>());
    }
}
