using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Trigger.Options;

/// <summary>
///     Automation run trigger polling options.
/// </summary>
public class TriggerPollingOptions
{
    /// <summary>
    ///     Wait time between polling.
    /// </summary>
    [Required]
    public TimeSpan PollingWait { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Timeout of a polling request.
    /// </summary>
    [Required]
    public TimeSpan PollingTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
