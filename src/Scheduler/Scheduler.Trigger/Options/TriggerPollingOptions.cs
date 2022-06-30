using Assistant.Net.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Trigger.Options;

/// <summary>
///     Automation run trigger polling options.
/// </summary>
public class TriggerPollingOptions
{
    /// <summary>
    ///     Time to delay between trigger polling.
    /// </summary>
    [Required, Time("00:00:00.001", "23:59:59")]
    public TimeSpan InactivityDelayTime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Delay between cancellation was requested and actually called.
    /// </summary>
    [Time("00:00:00", "00:30:00")]
    public TimeSpan CancellationDelay { get; set; } = TimeSpan.FromSeconds(30);
}
