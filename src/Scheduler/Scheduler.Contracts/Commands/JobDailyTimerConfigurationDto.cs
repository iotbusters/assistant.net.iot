using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Daily timer calculation strategy configuration.
/// </summary>
public class JobDailyTimerConfigurationDto : JobTimerConfigurationDto
{
    /// <summary/>
    public JobDailyTimerConfigurationDto(TimeSpan time, params DayOfWeek[] days)
    {
        if (!days.All(Enum.IsDefined))
            throw new ArgumentException("Invalid values were provided", nameof(days));

        Time = time;
        Days = days.Any() ? days : Enum.GetValues<DayOfWeek>();
        Array.Sort(Days);
    }

    /// <summary>
    ///     Time to trigger.
    /// </summary>
    public TimeSpan Time { get; }

    /// <summary>
    ///     Day of week list to trigger.
    /// </summary>
    public DayOfWeek[] Days { get; }

    /// <inheritdoc/>
    public override DateTimeOffset NextTriggerTime(DateTimeOffset lastTriggerTime)
    {
        var nextDay = Days.SkipWhile(x => x != lastTriggerTime.DayOfWeek).Skip(1).FirstOrDefault(Days.First());

        var dayDiff = (int)nextDay - (int)lastTriggerTime.DayOfWeek;
        if (dayDiff < 0)
            dayDiff += 7;
        return lastTriggerTime.AddDays(dayDiff).Add(Time - lastTriggerTime.TimeOfDay);
    }
}
