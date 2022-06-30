using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation job creation command.
/// </summary>
public sealed class JobCreateCommand : IMessage<Guid>, INonCaching
{
    /// <summary/>
    public JobCreateCommand(string name, JobConfigurationDto configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    /// <summary>
    ///     Name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Specific job strategy configuration.
    /// </summary>
    public JobConfigurationDto Configuration { get; }
}
