using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks;

public record TestMessage1(string Text) : IMessage<TestResponse>;