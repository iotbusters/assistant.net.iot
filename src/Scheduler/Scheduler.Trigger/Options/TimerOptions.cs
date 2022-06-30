using Assistant.Net.DataAnnotations;
using System;

namespace Assistant.Net.Scheduler.Trigger.Options;

/// <summary>
///     Timer triggering configuration options.
/// </summary>
public class TimerOptions
{
    /// <summary>
    ///     Time to delay if no triggers to schedule were found.
    /// </summary>
    [Time("00:00:01", "23:59:59")]
    public TimeSpan InactivityDelayTime { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Time to expire triggered timers.
    /// </summary>
    [Time("00:00:01")]
    public TimeSpan TriggeredTimerExpirationTime { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    ///     Delay between cancellation was requested and actually called.
    /// </summary>
    [Time("00:00:00", "00:30:00")]
    public TimeSpan CancellationDelay { get; set; } = TimeSpan.FromSeconds(30);
}
