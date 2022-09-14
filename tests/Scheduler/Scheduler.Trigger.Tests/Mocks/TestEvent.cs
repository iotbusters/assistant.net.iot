using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks;

public sealed record TestEvent : IMessage;

public sealed record TestEvent<T>(T Value) : IMessage;
