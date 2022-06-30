using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks;

public record TestEvent : IMessage;

public record TestEvent<T>(T Value) : IMessage;
