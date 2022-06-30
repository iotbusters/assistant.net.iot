using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation job trigger event create model.
/// </summary>
public class JobEventCreateModel : JobCreateModel
{
    /// <summary>
    ///     Triggered event name.
    /// </summary>
    public string? EventName { get; set; }

    /// <summary>
    ///     Event mask of <see cref="EventName"/>.
    /// </summary>
    public IDictionary<string, string>? EventMask { get; set; }
}
