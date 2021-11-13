using Assistant.Net.Abstractions;
using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Trigger.Handlers;
using Assistant.Net.Scheduler.Trigger.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Internal
{
    internal class TriggerListeningService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly IOptionsMonitor<TriggerOptions> options;
        private readonly IMessagingClient client;
        private readonly ITypeEncoder typeEncoder;
        
        public TriggerListeningService(
            ILogger<TriggerListeningService> logger,
            IOptionsMonitor<TriggerOptions> options,
            IMessagingClient client,
            ITypeEncoder typeEncoder)
        {
            this.logger = logger;
            this.options = options;
            this.client = client;
            this.typeEncoder = typeEncoder;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (true)
            {
                var events = options.CurrentValue.Events
                    .Select(x => (Type: typeEncoder.Decode(x.Name), x.Mask))
                    .Where(x => x.Type != null)
                    .ToDictionary(x => x.Type!, x => x.Mask);

                var services = new ServiceCollection()
                    .AddMongoMessageHandling(b =>
                    {
                        foreach (var @event in events)
                            b.AddHandler(typeof(GenericMessageHandler<,>).MakeGenericTypeBoundToMessage(@event.Key));
                    })
                    .BuildServiceProvider()
                    .GetServices<IHostedService>();

            }
        }
    }
}
