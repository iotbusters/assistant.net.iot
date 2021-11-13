using Assistant.Net.Abstractions;
using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Configuration;
using Assistant.Net.Scheduler.Trigger.Internal;
using Assistant.Net.Scheduler.Trigger.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Scheduler.Trigger
{
    /// <summary/>
    public class Startup
    {
        /// <summary>
        ///     Messaging MongoDB database name.
        /// </summary>
        public const string MessagingDatabaseName = "Scheduler";

        private static readonly ISet<string> unsupportedEvents = new HashSet<string>();

        /// <summary/>
        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        /// <summary/>
        public IConfiguration Configuration { get; }

        /// <summary/>
        public void ConfigureServices(IServiceCollection services) => services
            .AddHostedService<TriggerListeningService>()
            .Configure<TriggerOptions>(Configuration.GetSection(ConfigurationNames.TriggerSectionName))
            .AddMongoMessageHandling(b => b.Use(ConfigureMongo))
            .Configure<MongoHandlingServerOptions, IServiceProvider>((o, p) =>
            {
                o.MessageTypes.Clear();
                o.MessageTypes.AddRange(GetEventTypes(p));
            })
            .AddMessagingClient(b => b
                .UseMongo(ConfigureMongo)
                .AddMongo<TriggerReferencesQuery>()
                .AddMongo<TriggerQuery>());

        private void ConfigureMongo(MongoOptions options) => options
            .Connection(Configuration.GetConnectionString(ConfigurationNames.Messaging)).Database(MessagingDatabaseName);

        private static Type[] GetEventTypes(IServiceProvider provider)
        {
            var logger = provider.GetRequiredService<ILogger<Startup>>();
            var options = provider.GetRequiredService<IOptionsMonitor<TriggerOptions>>();

            var typeEncoder = provider.GetRequiredService<ITypeEncoder>();
            var events = options.CurrentValue.Events
                .Select(x => x.Name)
                .Except(unsupportedEvents)
                .Select(x => (name: x, type: typeEncoder.Decode(x)))
                .ToArray();

            var unknownEvents = events.Where(x => x.type == null).Select(x => x.name).ToArray();
            foreach (var eventName in unknownEvents)
            {
                unsupportedEvents.Add(eventName);
                logger.LogError("Unsupported {EventName}", eventName);
            }

            var knownEvents = events.Where(x => x.type != null).Select(x => x.type!).ToArray();
            if(!knownEvents.Any())
                logger.LogWarning("No supported events were found.");

            return knownEvents;
        }
    }
}
