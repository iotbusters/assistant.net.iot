
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Stopwatch timer calculation strategy configuration.
/// </summary>
public class JobStopwatchTimerConfigurationDto : JobTimerConfigurationDto
{
    /// <summary/>
    public JobStopwatchTimerConfigurationDto(TimeSpan time) =>
        Time = time;

    /// <summary>
    ///     Time delay before triggering.
    /// </summary>
    public TimeSpan Time { get; }

    /// <inheritdoc/>
    public override DateTimeOffset NextTriggerTime(DateTimeOffset lastTriggerTime) =>
        lastTriggerTime.Add(Time);
}
