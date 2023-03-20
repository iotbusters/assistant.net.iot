using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Handlers;

internal sealed class TriggerEventHandlers : IMessageHandler<TriggerCreatedEvent>, IMessageHandler<TriggerDeactivatedEvent>
{
    private readonly ITimerScheduler scheduler;
    private readonly IEventTriggerService service;

    public TriggerEventHandlers(ITimerScheduler scheduler, IEventTriggerService service)
    {
        this.scheduler = scheduler;
        this.service = service;
    }

    public async Task Handle(TriggerCreatedEvent @event, CancellationToken token)
    {
        await service.AddEventTrigger(@event.RunId, token);
        await scheduler.ScheduleTimer(@event.RunId, token);
    }

    public async Task Handle(TriggerDeactivatedEvent @event, CancellationToken token) =>
        await service.RemoveEventTrigger(@event.RunId, token);
}
