using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks;

public record TestMessage2(string Text) : IMessage<TestResponse>;