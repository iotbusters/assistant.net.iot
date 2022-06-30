using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Event trigger strategy configuration.
/// </summary>
public class JobEventConfigurationDto : JobConfigurationDto
{
    /// <summary/>
    public JobEventConfigurationDto(string eventName, IDictionary<string, string> eventMask)
    {
        EventName = eventName;
        EventMask = eventMask;
    }

    /// <summary>
    ///     Event name expectation.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    ///     Event body mask expectation.
    /// </summary>
    public IDictionary<string, string> EventMask { get; }
}
