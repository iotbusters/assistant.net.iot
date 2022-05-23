using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Handlers;

internal class RunSucceededEventHandler : IMessageHandler<RunSucceededEvent>
{
    private readonly ILogger logger;
    private readonly IMessagingClient client;

    public RunSucceededEventHandler(
        ILogger<RunSucceededEventHandler> logger,
        IMessagingClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task Handle(RunSucceededEvent @event, CancellationToken token)
    {
        var run = await client.Request(new RunQuery(@event.RunId), token);
        if (run.NextRunId != null)
        {
            logger.LogInformation("Run({AutomationId}/{RunId}): succeeded.", run.AutomationId, run.Id);
            return;
        }

        var nextRunId = await client.Request(new RunCreateCommand(run.AutomationId), token);
        logger.LogInformation("Run({AutomationId}/{RunId}): automation restarted.", run.AutomationId, nextRunId);
    }
}
