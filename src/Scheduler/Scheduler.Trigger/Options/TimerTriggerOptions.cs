using Assistant.Net.Abstractions;
using Assistant.Net.DataAnnotations;
using Assistant.Net.RetryStrategies;
using System;

namespace Assistant.Net.Scheduler.Trigger.Options;

/// <summary>
///     Timer triggering configuration options.
/// </summary>
public sealed class TimerTriggerOptions
{
    /// <summary>
    ///     Time to delay if no triggers to schedule were found.
    /// </summary>
    [Time("00:00:00.001", "23:59:59")]
    public TimeSpan InactivityDelayTime { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Time to expire triggered timers.
    /// </summary>
    [Time("00:00:01")]
    public TimeSpan TriggeredTimerExpirationTime { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    ///     Timer trigger scheduling retry strategy.
    /// </summary>
    public IRetryStrategy Retry { get; set; } = new ConstantBackoff {MaxAttemptNumber = 6, Interval = TimeSpan.FromSeconds(5)};
}
