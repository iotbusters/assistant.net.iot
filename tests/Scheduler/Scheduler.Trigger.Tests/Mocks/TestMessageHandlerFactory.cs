using Assistant.Net.Scheduler.Trigger.Abstractions;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Trigger.Tests.Mocks;

public class TestMessageHandlerFactory : IMessageHandlerFactory
{
    private readonly Dictionary<Type, object> handlers = new();

    public object Create(Type messageType) =>
        handlers.TryGetValue(messageType, out var value)
        ? value
        : throw new ArgumentException($"{messageType} wasn't arranged in a test.", nameof(messageType));

    public void Add(Type messageType, object handler) =>
        handlers[messageType] = handler;

    public void Remove(Type messageType) =>
        handlers.Remove(messageType);
}
