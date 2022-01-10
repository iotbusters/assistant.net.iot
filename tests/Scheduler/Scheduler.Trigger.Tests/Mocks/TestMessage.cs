using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks
{
    public record TestMessage(string Text) : IMessage<TestResponse>;
}
