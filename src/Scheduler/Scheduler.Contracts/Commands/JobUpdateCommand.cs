using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation job updating command.
/// </summary>
public sealed class JobUpdateCommand : IMessage, INonCaching
{
    /// <summary/>
    public JobUpdateCommand(Guid id, string name, JobConfigurationDto configuration)
    {
        Id = id;
        Name = name;
        Configuration = configuration;
    }

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     New name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     New specific strategy configuration.
    /// </summary>
    public JobConfigurationDto Configuration { get; }
}
