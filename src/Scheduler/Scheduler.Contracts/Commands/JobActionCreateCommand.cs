using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation job action creation command.
/// </summary>
public class JobActionCreateCommand : JobCreateCommand
{
    /// <summary/>
    public JobActionCreateCommand(
        string name,
        IMessage action) : base(name)
    {
        Action = action;
    }

    /// <summary>
    ///     Job action message.
    /// </summary>
    public IMessage Action { get; }
}
