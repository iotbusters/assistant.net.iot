using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation updating command.
/// </summary>
public sealed class AutomationUpdateCommand : IMessage, INonCaching
{
    /// <summary/>
    public AutomationUpdateCommand(Guid id, string name, JobReferenceDto[] jobs)
    {
        Id = id;
        Name = name;
        Jobs = jobs;
    }

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Job reference sequence.
    /// </summary>
    public JobReferenceDto[] Jobs { get; }
}
