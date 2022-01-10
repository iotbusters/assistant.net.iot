using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Trigger.Abstractions;

public interface IMessageHandlerFactory
{
    object Create(Type messageType);
}
