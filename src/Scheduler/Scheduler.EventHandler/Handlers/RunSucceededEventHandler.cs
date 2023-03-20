using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Handlers;

internal sealed class RunSucceededEventHandler : IMessageHandler<RunSucceededEvent>
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
        logger.LogDebug("Run({RunId}) orchestration: begins.", @event.RunId);

        var run = await client.Request(new RunQuery(@event.RunId), token);
        if (run.NextRunId != null)
        {
            await client.Request(new RunStartCommand(run.NextRunId.Value), token);
            logger.LogInformation("Run({AutomationId}, {RunId}) orchestration: ends the automation run.",
                run.AutomationId, run.Id);
            return;
        }

        logger.LogInformation("Run({AutomationId}, {RunId}) orchestration: ends and restarts the automation.", run.AutomationId, run.Id);

        var nextRunId = await client.Request(new RunCreateCommand(run.AutomationId), token);

        logger.LogInformation("Run({AutomationId}, {RunId}) orchestration: restarted the automation.",
            run.AutomationId, nextRunId);
    }
}
