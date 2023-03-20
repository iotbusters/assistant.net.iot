using Assistant.Net.Scheduler.Trigger.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Internal;

internal sealed class TimerTriggerService : BackgroundService
{
    private readonly ITimerScheduler scheduler;
    private readonly IEventTriggerService service;

    public TimerTriggerService(
        ITimerScheduler scheduler,
        IEventTriggerService service)
    {
        this.scheduler = scheduler;
        this.service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await service.ReloadEventTriggers(token);
        await scheduler.Start(token);
    }
}
