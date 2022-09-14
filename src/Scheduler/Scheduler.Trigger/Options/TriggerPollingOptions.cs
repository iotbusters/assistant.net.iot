using Assistant.Net.Abstractions;
using Assistant.Net.DataAnnotations;
using Assistant.Net.RetryStrategies;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Trigger.Options;

/// <summary>
///     Automation run trigger polling options.
/// </summary>
public sealed class TriggerPollingOptions
{
    /// <summary>
    ///     Time to delay between trigger polling.
    /// </summary>
    [Required, Time("00:00:00.001", "23:59:59")]
    public TimeSpan InactivityDelayTime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Trigger polling retry strategy.
    /// </summary>
    public IRetryStrategy Retry { get; set; } = new ConstantBackoff {MaxAttemptNumber = 6, Interval = TimeSpan.FromSeconds(5)};
}
