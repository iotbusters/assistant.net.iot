using System;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation job stopwatch timer create model.
/// </summary>
public sealed class JobStopwatchTimerCreateModel : JobCreateModel
{
    /// <summary>
    ///     Time delay before triggering.
    /// </summary>
    public TimeSpan Time { get; set; }
}
