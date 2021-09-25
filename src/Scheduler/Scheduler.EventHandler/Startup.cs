using Assistant.Net.Messaging;
using Assistant.Net.Scheduler.EventHandler.Internal;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<EventHandlingService>();

            services.AddStorage(b => b
                .UseMongo(Configuration.GetConnectionString("StorageDatabase"))
                .AddMongoPartitioned<Guid, _>());

            services
                .AddRemoteWebMessageHandler(mcb => mcb
                    .UseWebHandler(hcb => hcb
                        .ConfigureHttpClient(c =>
                        {
                            var configuration = Configuration.GetSection("HttpClient:RemoteMessageHandler");
                            // get path
                            c.BaseAddress = configuration.GetSection("BaseAddress").Get<Uri>()
                                            ?? throw new InvalidOperationException("'BaseAddress' is required for remote web handler.");
                            c.Timeout = configuration.GetSection("Timeout").Get<TimeSpan?>() ?? TimeSpan.FromSeconds(5);
                        }))
                    .AddConfiguration<RemoteWebMessageHandlersConfiguration>());
        }
    }
}
