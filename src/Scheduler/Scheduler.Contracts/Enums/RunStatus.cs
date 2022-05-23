namespace Assistant.Net.Scheduler.Contracts.Enums;

/// <summary>
///     Automation run instance status.
/// </summary>
public enum RunStatus
{
    /// <summary>
    ///     Run was scheduled.
    /// </summary>
    Scheduled = 1,

    /// <summary>
    ///     Run was started.
    /// </summary>
    Started,

    /// <summary>
    ///     Run was succeeded.
    /// </summary>
    Succeeded,

    /// <summary>
    ///     Run was failed.
    /// </summary>
    Failed
}
