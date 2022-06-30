using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Trigger time calculation strategy configuration.
/// </summary>
public abstract class JobTimerConfigurationDto : JobConfigurationDto
{
    /// <summary>
    ///     Calculate next trigger time.
    /// </summary>
    /// <param name="lastTriggerTime">Previous trigger time required to calculate next one.</param>
    public abstract DateTimeOffset NextTriggerTime(DateTimeOffset lastTriggerTime);
}
