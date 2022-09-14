using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Abstractions;

public interface IEventTriggerService
{
    Task ReconfigureEventTriggers(CancellationToken token);

    Task ConfigureEventTrigger(Guid runId, CancellationToken token);
}
