using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Handlers;

internal class TimerTriggeredEventHandler : IMessageHandler<TimerTriggeredEvent>
{
    private readonly ILogger logger;
    private readonly IMessagingClient client;

    public TimerTriggeredEventHandler(
        ILogger<TimerTriggeredEventHandler> logger,
        IMessagingClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task Handle(TimerTriggeredEvent @event, CancellationToken token)
    {
        var run = await client.Request(new RunQuery(@event.RunId), token);

        var succeeded = new RunStatusDto(RunStatus.Succeeded, $"Timer has triggered the run at {@event.Triggered}");
        await client.Request(new RunUpdateCommand(run.Id, status: succeeded), token);

        logger.LogInformation("Run({AutomationId}/{RunId}): succeeded.", run.AutomationId, run.Id);
    }
}
