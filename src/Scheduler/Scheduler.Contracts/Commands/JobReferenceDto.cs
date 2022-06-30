using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation job reference.
/// </summary>
public sealed class JobReferenceDto
{
    /// <summary/>
    public JobReferenceDto(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}
