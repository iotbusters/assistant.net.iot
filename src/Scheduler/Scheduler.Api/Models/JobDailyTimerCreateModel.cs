using System;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation job daily timer create model.
/// </summary>
public sealed class JobDailyTimerCreateModel : JobCreateModel
{
    /// <summary>
    ///     Time to trigger.
    /// </summary>
    public TimeSpan Time { get; set; }

    /// <summary>
    ///     Day of week list to trigger.
    /// </summary>
    public DayOfWeek[] Days { get; set; } = null!;
}
