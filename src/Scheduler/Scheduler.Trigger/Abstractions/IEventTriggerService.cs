using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Abstractions;

/// <summary>
///     Event trigger configuration service.
/// </summary>
public interface IEventTriggerService
{
    /// <summary>
    ///     Reloads event trigger configuration.
    /// </summary>
    Task ReloadEventTriggers(CancellationToken token);

    /// <summary>
    ///     Adds event trigger to configuration.
    /// </summary>
    /// <param name="runId">Automation run ID.</param>
    /// <param name="token"/>
    Task AddEventTrigger(Guid runId, CancellationToken token);

    /// <summary>
    ///     Removes event trigger to configuration.
    /// </summary>
    /// <param name="runId">Automation run ID.</param>
    /// <param name="token"/>
    Task RemoveEventTrigger(Guid runId, CancellationToken token);
}
