using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Models;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation run update model.
/// </summary>
public sealed class RunUpdateModel
{
    /// <summary>
    ///     Latest status of the run.
    /// </summary>
    public RunStatus Status { get; set; } = RunStatus.Scheduled;
}
