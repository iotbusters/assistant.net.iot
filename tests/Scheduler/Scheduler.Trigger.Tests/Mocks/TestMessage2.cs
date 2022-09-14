using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks;

public sealed record TestMessage2(string Text) : IMessage<TestResponse>;
