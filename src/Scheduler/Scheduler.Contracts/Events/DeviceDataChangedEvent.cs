using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events;

/// <summary>
///     Device data has been changed event.
/// </summary>
public sealed class DeviceDataChangedEvent : IMessage
{
    /// <summary>
    ///     Associated device ID.
    /// </summary>
    public Guid DeviceId { get; set; }
}