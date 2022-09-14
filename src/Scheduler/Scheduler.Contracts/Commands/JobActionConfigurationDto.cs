using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Action execution strategy configuration.
/// </summary>
public sealed class JobActionConfigurationDto : JobConfigurationDto
{
    /// <summary/>
    public JobActionConfigurationDto(IMessage action) =>
        Action = action;

    /// <summary>
    ///     Action message to request.
    /// </summary>
    public IMessage Action { get; }
}
