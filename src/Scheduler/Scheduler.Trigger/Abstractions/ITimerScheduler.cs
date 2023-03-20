using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Abstractions;

/// <summary>
///     Timer scheduling service according to automation run configuration.
/// </summary>
public interface ITimerScheduler
{
    /// <summary>
    ///     Starts the service by scheduling all active timers.
    /// </summary>
    Task Start(CancellationToken token);

    /// <summary>
    ///     Schedules new timer configured in the automation run.
    /// </summary>
    Task ScheduleTimer(Guid runId, CancellationToken token);
}
