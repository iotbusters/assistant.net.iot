using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Trigger.Options;

/// <summary>
///     Listening trigger event details.
/// </summary>
[Obsolete("replaced with TriggerEventSource")]
public class TriggerEvent
{
    /// <summary>
    ///     Event name.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Event body mask.
    /// </summary>
    [Required]
    public IDictionary<string, string> Mask { get; set; } = null!;
}