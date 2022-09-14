using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Handlers;

internal sealed class TriggerCreatedEventHandler : IMessageHandler<TriggerCreatedEvent>
{
    private readonly ITimerScheduler scheduler;
    private readonly IEventTriggerService service;

    public TriggerCreatedEventHandler(
        ITimerScheduler scheduler,
        IEventTriggerService service)
    {
        this.scheduler = scheduler;
        this.service = service;
    }

    public async Task Handle(TriggerCreatedEvent @event, CancellationToken token)
    {
        await service.ConfigureEventTrigger(@event.RunId, token);
        await scheduler.ScheduleTimer(@event.RunId, token);
    }
}
