//using Assistant.Net.Messaging.Abstractions;
//using Assistant.Net.Scheduler.Contracts.Commands;
//using Assistant.Net.Scheduler.Contracts.Events;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Assistant.Net.Scheduler.EventHandler.Handlers;

//internal sealed class TimerTriggeredEventHandler : IMessageHandler<TimerTriggeredEvent>
//{
//    private readonly IMessagingClient client;

//    public TimerTriggeredEventHandler(IMessagingClient client) =>
//        this.client = client;

//    public async Task Handle(TimerTriggeredEvent @event, CancellationToken token) =>
//        await client.Request(new RunSucceedCommand(@event.RunId), token);
//}
