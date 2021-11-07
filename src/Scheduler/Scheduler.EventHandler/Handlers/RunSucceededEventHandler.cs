using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Handlers
{
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
            var completedRun = await client.Request(new RunQuery(@event.RunId), token);
            if (completedRun.NextRunId == null)
            {
                var nextRunId = await client.Request(new RunCreateCommand(completedRun.AutomationId), token);
                logger.LogInformation("Run({AutomationId}/{RunId}): automation restarted.", completedRun.AutomationId, nextRunId);
                return;
            }

            var nextRun = await client.Request(new RunQuery(id: completedRun.NextRunId.Value), token);
            switch (nextRun.JobSnapshot)
            {
                case JobTriggerModel:
                    var started = new RunStatusDto(RunStatus.Started);
                    await client.Request(new RunUpdateCommand(nextRun.Id, status: started), token);
                    logger.LogInformation("Run({AutomationId}/{RunId}): started.", nextRun.AutomationId, nextRun.Id);
                    break;
                case JobActionModel model:
                    await client.Request(model.Action, token);
                    var succeeded = new RunStatusDto(RunStatus.Succeeded);
                    await client.Request(new RunUpdateCommand(nextRun.Id, status: succeeded), token);
                    logger.LogInformation("Run({AutomationId}/{RunId}): completed.", nextRun.AutomationId, nextRun.Id);
                    break;
                default:
                    var jobTypeNotExpected = $"{nextRun.JobSnapshot.GetType().Name} isn't expected.";
                    var failed = new RunStatusDto(RunStatus.Failed, jobTypeNotExpected);
                    await client.Request(new RunUpdateCommand(nextRun.Id, status: failed), token);
                    logger.LogInformation("Run({AutomationId}/{RunId}): failed.", nextRun.AutomationId, nextRun.Id);
                    break;
            }
            
        }
    }
}
