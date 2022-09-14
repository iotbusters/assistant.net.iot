using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Abstractions;

public interface ITimerScheduler
{
    Task RescheduleTimers(CancellationToken token);

    Task ScheduleTimer(Guid runId, CancellationToken token);
}
