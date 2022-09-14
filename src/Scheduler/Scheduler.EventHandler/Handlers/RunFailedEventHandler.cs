using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Handlers;

internal sealed class RunFailedEventHandler : IMessageHandler<RunFailedEvent>
{
    private readonly ILogger logger;
    private readonly IMessagingClient client;

    public RunFailedEventHandler(ILogger<RunFailedEventHandler> logger, IMessagingClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task Handle(RunFailedEvent @event, CancellationToken token)
    {
        var run = await client.Request(new RunQuery(@event.RunId), token);
        logger.LogCritical("Run({AutomationId}, {RunId}): failed.", run.AutomationId, run.Id);
        // note: automation is implicitly blocked.
        // todo: resolve run failure and automation blocking.
    }
}
