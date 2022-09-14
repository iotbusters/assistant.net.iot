using System;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Trigger.Options;

/// <summary>
///     Event triggering configuration options.
/// </summary>
public sealed class EventTriggerOptions
{
    /// <summary>
    ///     List of monitoring published event-messages to trigger associated run.
    /// </summary>
    public IDictionary<Type, IList<Guid>> EventTriggers { get; set; } = null!;
}
